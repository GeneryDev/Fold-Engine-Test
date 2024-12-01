using FoldEngine.Components;
using FoldEngine.Gui.Components.Controls;

namespace FoldEngine.Gui.Components;

[Component("fold:control.popup_provider")]
public struct PopupProvider
{
    public MouseActionMode ActionMode;
    public MouseButtonMask ButtonMask;
}