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

[Event("fold:control.drop_validation_requested", EventFlushMode.Immediate)]
public struct DropValidationRequestedEvent
{
    [EntityId] public long TargetEntityId = -1;
    [EntityId] public long DragOperationEntityId = -1;
    public bool CanDrop = false;

    public DropValidationRequestedEvent()
    {
    }
}

[Event("fold:control.drag_operation_canceled", EventFlushMode.Immediate)]
public struct DragOperationCanceledEvent
{
    [EntityId] public long TargetEntityId = -1;
    [EntityId] public long DragOperationEntityId = -1;

    public DragOperationCanceledEvent()
    {
    }
}

[Event("fold:control.dropped_data", EventFlushMode.Immediate)]
public struct DroppedDataEvent
{
    [EntityId] public long TargetEntityId = -1;
    [EntityId] public long DragOperationEntityId = -1;
    public bool Consumed = false;

    public DroppedDataEvent()
    {
    }
}