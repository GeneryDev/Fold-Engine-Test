using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:control.tab_selected")]
public struct TabSelectedEvent
{
    [EntityId] public long TabListId;
    [EntityId] public long TabId;
}