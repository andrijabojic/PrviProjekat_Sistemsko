using DotNetEnv;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

class Server
{
    private Cache _cache;
    private HttpListener _listener;
    private HttpClient _httpClient;
    private string _apiKey;
    private bool _running = false;
    private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>(); //za sprecavanje stampeda
    private CustomQueue _queue = new CustomQueue();
    public Server(int capacity, int workerCount)
    {
        _running = true;
        _cache = new Cache(capacity);
        DotNetEnv.Env.Load();
        _listener = new HttpListener();
        _httpClient = new HttpClient();
        _apiKey = DotNetEnv.Env.GetString("API_KEY");
        for (int i = 0; i < workerCount; i++)
        {
            Thread worker = new Thread(WorkerWork);
            worker.IsBackground = true;
            worker.Start();
        }
    }
    public void Start()
    {
        _listener.Prefixes.Add("http://localhost:8080/");
        _listener.Start();
        Thread inputThread = new Thread(Exit);
        inputThread.IsBackground = true;
        inputThread.Start();
        Console.WriteLine("Pritisni bilo koje dugme za gasenje");
        while (_running)
        {
            try
            {
                HttpListenerContext context = _listener.GetContext();
                _queue.Enqueue(context);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
    private void Exit()
    {
        Console.ReadKey(true);
        Console.WriteLine("Gasim server...");
        _running = false;
        _listener.Stop();
        _listener.Close();
        _queue.NotifyAll();
    }
    private void WorkerWork()
    {
        while (_running)
        {
            try
            {
                HttpListenerContext context = _queue.Dequeue();

                if (context == null) continue;

                string city = context.Request.Url.AbsolutePath.Trim('/');
                if (string.IsNullOrEmpty(city) || city.ToLower() == "favicon.ico")
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Close();
                    continue;
                }

                if (string.IsNullOrEmpty(city))
                {
                    SendResponse(context, "{\"error\": \"Grad nije naveden\"}", HttpStatusCode.BadRequest);
                    continue;
                }

                string jsonResult = _cache.Get(city);
                if (jsonResult != null)
                {
                    Log.Input($"Pogodak kesa! Podaci za {city} poslati iz memorije.");
                    SendResponse(context, jsonResult, HttpStatusCode.OK);
                }
                else
                {
                    object cityLock = _locks.GetOrAdd(city, new object());

                    lock (cityLock)
                    {
                        jsonResult = _cache.Get(city); //ako je u medjuvremenu neka druga nit kesirala city
                        
                        if (jsonResult == null)
                        {
                            Log.Input($"Promasaj kesa! Zovem API za {city}...");
                            jsonResult = FetchFromWeatherApi(city);
                            if(jsonResult != null)
                            {
                                jsonResult = ParseJSON(jsonResult);
                                _cache.Add(city, jsonResult);
                            }
                        }
                    }
                    if(jsonResult != null)
                    {
                        SendResponse(context, jsonResult, HttpStatusCode.OK);
                    }
                    else
                    {
                        SendResponse(context, "{\"error\": \"API nedostupan\"}", HttpStatusCode.InternalServerError);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska u radnoj niti: {ex.Message}");
            }
        }
    }

    private void SendResponse(HttpListenerContext context, string responseString, HttpStatusCode statusCode)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = buffer.Length;

        try
        {
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greska pri slanju: {ex.Message}");
        }
        finally
        {
            context.Response.Close();
        }
    }

    private string FetchFromWeatherApi(string city)
    {
        string url = $"http://api.weatherapi.com/v1/current.json?key={_apiKey}&q={city}&aqi=yes";
        try
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Console.WriteLine($"Problem sa zahtevom: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neuspesno povezivanje: {ex.Message}");
            return null;
        }
    }
    private string ParseJSON(string rawjson)
    {
        JObject json = JObject.Parse(rawjson);

        string grad = json["location"]["name"].ToString();
        string zemlja = json["location"]["country"].ToString();
        string vreme = json["location"]["localtime"].ToString();

        double temp = (double)json["current"]["temp_c"];
        int vlaznost = (int)json["current"]["humidity"];
        double vetar = (double)json["current"]["wind_kph"];
        double pm25 = (double)json["current"]["air_quality"]["pm2_5"];
        double pm10 = (double)json["current"]["air_quality"]["pm10"];

        string opis = json["current"]["condition"]["text"].ToString();

        return $"{{\"grad\":\"{grad}\", \"temp\":{temp}, \"opis\":\"{opis}\", \"pm25\":{pm25}, \"pm10\":{pm10}}}";
    }
}