using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:control.drag_data_requested", EventFlushMode.Immediate)]
public struct DragDataRequestedEvent
{
    [EntityId] public long SourceEntityId = -1;
    [EntityId] public long DragOperationEntityId = -1;
    public bool HasData = false;

    public DragDataRequestedEvent()
    {
    }
}