using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:focus.requested")]
public struct FocusRequestedEvent
{
    [EntityId] public long EntityId = -1;

    public FocusRequestedEvent()
    {
    }
}

[Event("fold:focus.gained")]
public struct FocusGainedEvent
{
    [EntityId] public long EntityId = -1;

    public FocusGainedEvent()
    {
    }
}

[Event("fold:focus.lost")]
public struct FocusLostEvent
{
    [EntityId] public long EntityId = -1;

    public FocusLostEvent()
    {
    }
}