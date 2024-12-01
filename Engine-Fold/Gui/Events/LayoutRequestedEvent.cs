using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:control.layout_requested", EventFlushMode.Immediate)]
public struct LayoutRequestedEvent
{
    [EntityId] public long EntityId;
    [EntityId] public long ViewportId;

    public LayoutRequestedEvent()
    {
    }
    
    public LayoutRequestedEvent(long entityId, long viewportId)
    {
        EntityId = entityId;
        ViewportId = viewportId;
    }
}