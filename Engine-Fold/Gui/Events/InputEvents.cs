using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:handle_inputs", EventFlushMode.Immediate)]
public struct HandleInputsEvent
{
    [EntityId] public long EntityId = -1;

    public HandleInputsEvent()
    {
    }
}