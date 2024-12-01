using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:control.minimum_size_requested", EventFlushMode.Immediate)]
public struct MinimumSizeRequestedEvent
{
    [EntityId] public long EntityId;
    [EntityId] public long ViewportId;

    public MinimumSizeRequestedEvent()
    {
    }
    
    public MinimumSizeRequestedEvent(long entityId, long viewportId)
    {
        EntityId = entityId;
        ViewportId = viewportId;
    }
}