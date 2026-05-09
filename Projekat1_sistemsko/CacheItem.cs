using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

class CacheItem
{
    private string _JSON;
    private int _count;
    public CacheItem(string json)
    {
        _JSON = json;
        _count = 1;
    }
    public void IncCount() { _count++; }
    public string JSON {
        get { return _JSON; }
        set { _JSON = value; }
    }
    public int Count
    {
        get { return _count; }
    }
}
