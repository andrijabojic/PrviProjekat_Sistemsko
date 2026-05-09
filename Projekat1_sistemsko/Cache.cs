using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

class Cache
{
    private int _capacity;
    private object _lock = new object();
    private Dictionary<string, CacheItem> _storage = new Dictionary<string, CacheItem>();

    public Cache(int capacity)
    {
        this._capacity = capacity;
    }
    //Za kes koristim LFU 
    public string Get(string key)
    {
        lock (_lock)
        {
            if (_storage.ContainsKey(key))
            {
                _storage[key].IncCount();
                return _storage[key].JSON;
            }
            return null;
        }
    }
    public void Add(string key, string json)
    {
        lock (_lock)
        {
            if (_storage.Count >= _capacity)
            {
                string minItem = _storage.OrderBy(x => x.Value.Count).First().Key;
                Console.WriteLine($"Kes je pun! Izbacujemo {minItem} iz kesa");
                _storage.Remove(minItem);
            }
            _storage[key] = new CacheItem(json);
        }
    }
}