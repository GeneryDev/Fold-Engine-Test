using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.Systems;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.popups", ProcessingCycles.Update, true)]
public class ControlPopupSystem : GameSystem
{
    private ComponentIterator<Popup> _popups;

    public override void Initialize()
    {
        _popups = CreateComponentIterator<Popup>(IterationFlags.None);
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref MouseButtonEvent evt) =>
        {
            long clickedPopupId = GetPopupForEntity(evt.EntityId);
            
            _popups.Reset();
            while (_popups.Next())
            {
                ref var popup = ref _popups.GetComponent();
                long popupId = _popups.GetEntityId();
                var dismiss = false;

                bool inside = clickedPopupId == popupId;
                bool outside = !inside;
                if (inside && (popup.DismissOnClick & Popup.PopupClickCondition.Inside) != 0) dismiss = true;
                if (outside && (popup.DismissOnClick & Popup.PopupClickCondition.Outside) != 0) dismiss = true;

                if (dismiss)
                {
                    Scene.Events.Invoke(new PopupDismissalRequested()
                    {
                        PopupEntityId = popupId
                    });
                }
            }
        });
        Subscribe((ref PopupDismissalRequested evt) =>
        {
            if (!Scene.Components.HasComponent<Popup>(evt.PopupEntityId)) return;
            Scene.DeleteEntity(evt.PopupEntityId, recursively: true);
        });
    }

    private long GetPopupForEntity(long entityId)
    {
        while (entityId != -1)
        {
            if (Scene.Components.HasComponent<Popup>(entityId)) return entityId;
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);

            entityId = hierarchical.ParentId;
        }

        return -1;
    }
}