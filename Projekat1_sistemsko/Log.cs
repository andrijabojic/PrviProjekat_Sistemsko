using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

static class Log
{
    private static object _lock = new object();
    private static string _path = "log.txt";
    public static void Input(string input)
    {
        lock (_lock)
        {
            Console.WriteLine(input);

            try
            {
                File.AppendAllText(_path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {input}" + Environment.NewLine);
            }
            catch {}
        }
    }
}
