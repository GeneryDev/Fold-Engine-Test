using FoldEngine.Components;
using FoldEngine.Editor.Views;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Serialization;

namespace FoldEngine.Editor.ImmediateGui;

[Component("fold:control.immediate_gui", traits: [typeof(Control), typeof(MouseFilterDefaultStop)])]
public struct ImmediateGuiControl
{
    [DoNotSerialize]
    public EditorView View;
}