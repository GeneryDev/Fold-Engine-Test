using FoldEngine.Components;
using FoldEngine.Editor.Views;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.ImmediateGui;
using FoldEngine.Serialization;

namespace FoldEngine.Editor.ImmediateGui;

[Component("fold:control.immediate_gui", traits: [typeof(Control), typeof(MouseFilterDefaultStop)])]
public struct ImmediateGuiControl
{
    [DoNotSerialize]
    public EditorEnvironment Environment;
    [DoNotSerialize]
    public EditorView View;
    [DoNotSerialize]
    public GuiPanel ContainerPanel;
}