using FoldEngine.Events;

namespace FoldEngine.Gui.Events;

[Event("fold:control.minimum_size_requested", EventFlushMode.Immediate)]
public struct MinimumSizeRequestedEvent
{
    public long EntityId;
    public long ViewportId;

    public MinimumSizeRequestedEvent()
    {
    }
    
    public MinimumSizeRequestedEvent(long entityId, long viewportId)
    {
        EntityId = entityId;
        ViewportId = viewportId;
    }
}