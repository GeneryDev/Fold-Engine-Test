using System;

namespace FoldEngine.Events;

public delegate void EventListener<T>(ref T evt);

public sealed class EventAttribute : Attribute
{
    public readonly string EventIdentifier;
    public readonly EventFlushMode FlushMode;

    public EventAttribute(string eventIdentifier, EventFlushMode flushMode = EventFlushMode.AfterSystem)
    {
        EventIdentifier = eventIdentifier;
        FlushMode = flushMode;
    }
}

public enum EventFlushMode
{
    Immediate,
    AfterSystem,
    End
}