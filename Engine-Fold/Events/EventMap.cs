using System;
using System.Collections.Generic;

namespace FoldEngine.Events;

public class EventMap
{
    /// <summary>
    ///     The map containing the event queues.<br></br>
    /// </summary>
    internal readonly Dictionary<Type, IEventQueue> Map = new Dictionary<Type, IEventQueue>();

    private EventScheduler _afterSystemScheduler = new();
    private EventScheduler _endScheduler = new();

    /// <summary>
    ///     Creates an EventMap.
    /// </summary>
    public EventMap()
    {
    }

    private EventQueue<T> Get<T>() where T : struct
    {
        if (!Map.ContainsKey(typeof(T)))
        {
            var queue = new EventQueue<T>();
            Map[typeof(T)] = queue;
        }

        return (EventQueue<T>)Map[typeof(T)];
    }

    public bool HasListeners<T>() where T : struct
    {
        return Map.ContainsKey(typeof(T)) && Map[typeof(T)].AnyListeners();
    }

    public T Invoke<T>(T evt) where T : struct
    {
        if (HasListeners<T>())
        {
            var queue = Get<T>();
            ScheduleFlush(queue);
            return queue.Enqueue(evt);
        }
        return evt;
    }

    private void ScheduleFlush(IEventQueue queue)
    {
        switch (queue.EventAttribute.FlushMode)
        {
            case EventFlushMode.AfterSystem:
            {
                _afterSystemScheduler.Schedule(queue);
                break;
            }
            case EventFlushMode.End:
            {
                _endScheduler.Schedule(queue);
                break;
            }
            case EventFlushMode.Immediate:
            default:
                break;
        }
    }

    public EventUnsubscriber Subscribe<T>(EventListener<T> listener) where T : struct
    {
        return Get<T>().Subscribe(listener);
    }

    public void FlushAfterSystem()
    {
        _afterSystemScheduler.Flush();
    }

    public void FlushEnd()
    {
        _endScheduler.Flush();
    }
}