using FoldEngine.Components;

namespace FoldEngine.Gui.Components.Controls;

[Component("fold:dismiss_popup_on_press")]
[ComponentInitializer(typeof(DismissPopupOnPress))]
public struct DismissPopupOnPress
{
    public long PopupId = -1;

    public DismissPopupOnPress()
    {
    }
}