using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoldEngine.Events;

public interface IEventQueue
{
    void Flush();
    void Unsubscribe(object listener);
    bool AnyListeners();
}

public class EventQueue<T> : IEventQueue where T : struct
{
    private const int StartingSize = 4;

    internal readonly EventAttribute EventAttribute;

    private T[] _events; //resized immediately, holds all queued events
    private int _flushIndex;

    private int _insertionIndex;

    private readonly List<EventListener<T>> _listeners = new List<EventListener<T>>();

    public EventQueue()
    {
        EventAttribute = typeof(T).GetCustomAttribute<EventAttribute>();
        _events = new T[StartingSize];
    }

    public void Flush()
    {
        _flushIndex = 0;
        while (_flushIndex < _insertionIndex)
        {
            foreach (EventListener<T> listener in _listeners) listener(ref _events[_flushIndex]);

            _flushIndex++;
        }

        _insertionIndex = 0;
    }

    public void Unsubscribe(object listener)
    {
        _listeners.Remove(listener as EventListener<T>);
    }

    public bool AnyListeners()
    {
        return _listeners.Count > 0;
    }

    public T Enqueue(T evt)
    {
        // If flush mode is immediate, invoke all listeners.
        if (EventAttribute.FlushMode == EventFlushMode.Immediate)
        {
            foreach (EventListener<T> listener in _listeners)
            {
                // Use the local event variable reference to hold the event in the stack, rather than the array.
                // This is necessary so that recursive event calls won't trigger the array resizing and result
                // in prior methods that haven't returned losing the event reference.
                listener(ref evt);
            }

            // Return a copy of the event stored in the stack,
            // so that systems hoping to invoke events for other systems to modify,
            // and get results immediately, can do so.
            return evt;
        }

        // If there are no listeners, do nothing else.
        if (_listeners.Count == 0)
        {
            return evt;
        }
        
        // Add the event to the array (resize if needed)
        // even if there are no listeners - this is so we can return a reference to it.
        if (_insertionIndex >= _events.Length)
        {
            Console.WriteLine("Resizing event array");
            Array.Resize(ref _events, _events.Length * 2);
        }

        _events[_insertionIndex] = evt;

        // The event was added, with the intention of adding more later.
        // Increment the insertion index.
        _insertionIndex++;
        
        return evt;
    }

    public static EventQueue<T> operator +(EventQueue<T> queue, EventListener<T> listener)
    {
        queue._listeners.Add(listener);
        return queue;
    }

    public EventUnsubscriber Subscribe(EventListener<T> listener)
    {
        _listeners.Add(listener);
        return new EventUnsubscriber
        {
            EventQueue = this,
            Listener = listener
        };
    }
}

public struct EventUnsubscriber
{
    internal IEventQueue EventQueue;
    internal object Listener;

    public void Unsubscribe()
    {
        EventQueue.Unsubscribe(Listener);
    }
}