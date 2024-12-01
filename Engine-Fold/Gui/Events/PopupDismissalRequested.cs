using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Events;

[Event("fold:control.popup_dismissal_requested")]
public struct PopupDismissalRequested
{
    [EntityId] public long PopupEntityId = -1;

    public PopupDismissalRequested()
    {
    }
}