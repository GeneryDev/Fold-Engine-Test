using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Events;

// TODO test without immediate
[Event("fold:editor.action_triggered", EventFlushMode.Immediate)]
public struct EditorActionTriggered
{
    [EntityId] public long EntityId = -1;

    public EditorActionTriggered()
    {
    }
}