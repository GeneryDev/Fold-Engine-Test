using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Scenes;

namespace FoldEngine.Editor;

[Component("fold:editor.tab", traits: [typeof(DragOperationStarter)])]
public struct EditorTab
{
}

[Component("fold:editor.tab_drop_target")]
public struct EditorTabDropTarget
{
    [EntityId] public long TabBarId;
}

[Component("fold:drag_data.editor.tab", traits: [typeof(DragData)])]
public struct EditorTabDragData
{
    [EntityId] public long TabId;
}