using FoldEngine.Components;
using FoldEngine.Editor.Components.Traits;
using FoldEngine.ImmediateGui;

namespace FoldEngine.Editor.Components;

[Component("fold:editor.legacy_action", traits: [typeof(EditorAction)])]
public struct LegacyActionComponent
{
    public IGuiAction Action;
    public GuiElement Element;
}