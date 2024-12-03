using FoldEngine.Components;
using FoldEngine.Editor.Components.Traits;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Components;

[Component("fold:editor.entity_action", traits: [typeof(EditorAction)])]
public struct EntityActionComponent
{
    public ActionType Type;
    public long AffectedEntityId;

    public enum ActionType
    {
        None,
        CreateChild,
        Delete
    }
}