using FoldEngine.Events;

namespace FoldEngine.Gui;

[Event("fold:control.layout_requested", EventFlushMode.Immediate)]
public struct LayoutRequestedEvent
{
    public long EntityId;
    public long ViewportId;

    public LayoutRequestedEvent()
    {
    }
    
    public LayoutRequestedEvent(long entityId, long viewportId)
    {
        EntityId = entityId;
        ViewportId = viewportId;
    }
}