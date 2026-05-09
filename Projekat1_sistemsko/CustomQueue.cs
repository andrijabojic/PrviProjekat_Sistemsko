using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

class CustomQueue
{
    private Queue<HttpListenerContext> _queue = new Queue<HttpListenerContext>();
    private object _lock = new object();
    public void Enqueue(HttpListenerContext item)
    {
        lock (_lock)
        {
            _queue.Enqueue(item);
            Monitor.Pulse(_lock);
        }
    }

    public HttpListenerContext Dequeue()
    {
        lock (_lock)
        {
            while (_queue.Count == 0)
            {
                Monitor.Wait(_lock);
            }
            return _queue.Dequeue();
        }
    }
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }
    public void NotifyAll()
    {
        lock (_lock)
        {
            Monitor.PulseAll(_lock);
        }
    }
}
