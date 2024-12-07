using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Events;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

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
        // Create popups from providers
        Subscribe((ref MouseButtonEvent evt) =>
        {
            if (evt.Consumed) return;
            if (!Scene.Components.HasComponent<PopupProvider>(evt.EntityId)) return;
            var provider = Scene.Components.GetComponent<PopupProvider>(evt.EntityId);
            if (((int)provider.ButtonMask & (1 << evt.Button)) == 0) return;
            if (provider.ActionMode == MouseActionMode.Press && evt.Type != MouseButtonEventType.Pressed) return;
            if (provider.ActionMode == MouseActionMode.Release && evt.Type != MouseButtonEventType.Released) return;

            Console.WriteLine($"Attempt create popup for {evt.EntityId}");
            CreatePopup(evt.EntityId, evt.Position);
            evt.Consume();
        });
        
        // Dismiss popups
        Subscribe((ref MouseButtonEvent evt) =>
        {
            if (evt.Type != MouseButtonEventType.Pressed) return;
            
            // Even if event consumed, do this check!
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
                    if(popup.ConsumeClickOnDismiss)
                        evt.Consume();
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

    public long CreatePopup(long providerId, Point globalMousePos, bool sendBuildRequestEvent = true, Point offset = default)
    {
        var popupEntity = Scene.CreateEntity("Popup");
        popupEntity.AddComponent<Control>() = new Control
        {
            RequestLayout = true,
            MouseFilter = Control.MouseFilterMode.Ignore
        };
        popupEntity.AddComponent<Popup>() = new Popup()
        {
            SourceEntityId = providerId,
            DismissOnClick = Popup.PopupClickCondition.Outside,
            ConsumeClickOnDismiss = true
        };
        popupEntity.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };

        if (sendBuildRequestEvent && providerId != -1)
        {
            var buildEvt = Scene.Events.Invoke(new PopupBuildRequestedEvent()
            {
                SourceEntityId = providerId,
                TooltipEntityId = popupEntity.EntityId,
                Position = GetLocalMousePos(providerId, globalMousePos),
                GlobalPosition = globalMousePos,
                Gap = offset
            });
            offset = buildEvt.Gap;
        }
        
        popupEntity.AddComponent<PopupContainer>() = new PopupContainer()
        {
            PopupPosition = globalMousePos.ToVector2(),
            Gap = offset.ToVector2()
        };

        return popupEntity.EntityId;
    }

    private Point GetLocalMousePos(long entityId, Point globalMousePos)
    {
        if (Scene.Components.HasComponent<Control>(entityId) && Scene.Components.HasComponent<Transform>(entityId))
        {
            return (globalMousePos.ToVector2() - Scene.Components.GetComponent<Transform>(entityId).Position)
                .ToPoint();
        }
        return globalMousePos;
    }

    private long GetPopupForEntity(long entityId)
    {
        while (entityId != -1)
        {
            if (Scene.Components.HasComponent<Popup>(entityId)) return entityId;
            if (!Scene.Components.HasComponent<Hierarchical>(entityId)) return -1;
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);

            entityId = hierarchical.ParentId;
        }

        return -1;
    }
}