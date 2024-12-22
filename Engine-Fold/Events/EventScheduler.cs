using System.Collections.Generic;

namespace FoldEngine.Events;

public struct EventScheduler
{
    private List<IEventQueue> _queues;

    public EventScheduler()
    {
        _queues = new List<IEventQueue>();
    }

    public void Schedule(IEventQueue queue)
    {
        _queues.Add(queue);
    }

    public void Flush()
    {
        int flushCount = _queues.Count;
        for (var i = 0; i < flushCount; i++)
        {
            _queues[i].FlushOne();
        }
        _queues.RemoveRange(0, flushCount);
    }
}