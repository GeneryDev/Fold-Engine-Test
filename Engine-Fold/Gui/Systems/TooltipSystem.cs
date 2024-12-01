using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Events;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.tooltips", ProcessingCycles.Update)]
public class TooltipSystem : GameSystem
{
    public float TooltipDelaySec = 0.5f;
    
    private long _hoveredProviderId = -1;
    private Point _hoverMousePos;
    private float _hoverTimer = 0;
    private string _hoverText = "";
    private long _popupEntityId = -1;
    private bool TooltipVisible => _popupEntityId != -1;
    
    public override void SubscribeToEvents()
    {
        Subscribe((ref MouseEnteredEvent evt) =>
        {
            _hoverMousePos = evt.Position;
            StartHovering(evt.EntityId, evt.Position);
        });
        Subscribe((ref MouseMovedEvent evt) =>
        {
            _hoverMousePos = evt.Position;
            StartHovering(evt.EntityId, evt.Position);
        });
        SubscribeToSimpleTooltipEvents();
    }

    private void SubscribeToSimpleTooltipEvents()
    {
        Subscribe((ref TooltipRequestedEvent evt) =>
        {
            if (Scene.Components.HasComponent<SimpleTooltip>(evt.EntityId))
            {
                var component = Scene.Components.GetComponent<SimpleTooltip>(evt.EntityId);
                evt.TooltipText = component.Text;
            }
        });
        Subscribe((ref TooltipBuildRequestedEvent evt) =>
        {
            if (Scene.Components.HasComponent<SimpleTooltip>(evt.SourceEntityId))
            {
                var label = Scene.CreateEntity("Simple Tooltip Label");
                label.AddComponent<Control>() = new Control()
                {
                    ZOrder = 100,
                    MouseFilter = Control.MouseFilterMode.Ignore,
                    RequestLayout = true
                };
                label.AddComponent<LabelControl>() = new LabelControl()
                {
                    Text = evt.TooltipText,
                    FontSize = 9,
                };
                label.AddComponent<AnchoredControl>() = new AnchoredControl()
                {
                    AnchorRight = 1,
                    AnchorLeft = 1,
                    GrowHorizontal = AnchoredControl.GrowDirection.End,
                    GrowVertical = AnchoredControl.GrowDirection.End
                };
                label.Hierarchical.SetParent(evt.TooltipEntityId);
                
                var bg = Scene.CreateEntity("Simple Tooltip Bg");
                bg.AddComponent<Control>() = new Control()
                {
                    ZOrder = 99,
                    MouseFilter = Control.MouseFilterMode.Ignore,
                    RequestLayout = true
                };
                bg.AddComponent<BoxControl>().Color = new Color(0, 0, 0, 200);
                bg.AddComponent<AnchoredControl>() = new AnchoredControl()
                {
                    AnchorRight = 1,
                    AnchorBottom = 1,
                    OffsetLeft = -8,
                    OffsetRight = 8,
                    OffsetTop = -4,
                    OffsetBottom = 4
                };
                bg.Hierarchical.SetParent(label);
                
                // var background

                evt.Offset = new Point(20, 10);
            }
        });
    }
    
    private void StartHovering(long entityId, Point globalMousePos)
    {
        if (_popupEntityId != -1 && IsEntityInPopup(entityId, _popupEntityId)) return;
        
        if (!Scene.Components.HasTrait<TooltipProvider>(entityId))
        {
            entityId = -1;
        }

        if (_hoveredProviderId != -1 && _hoveredProviderId != entityId)
        {
            StopHovering();
        }

        _hoveredProviderId = entityId;
        _hoverTimer = 0;

        if (TooltipVisible)
        {
            var newTooltip = RequestTooltipText(entityId, globalMousePos);
            if (newTooltip != _hoverText)
            {
                DismissTooltip();
            }
        }
    }

    private bool IsEntityInPopup(long entityId, long popupEntityId)
    {
        if (popupEntityId == -1) return false;
        while (entityId != -1)
        {
            if (entityId == popupEntityId) return true;
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);
            entityId = hierarchical.ParentId;
        }
        return false;
    }

    private void AttemptShowTooltip()
    {
        if (TooltipVisible) return;
        string tooltipText = RequestTooltipText(_hoveredProviderId, _hoverMousePos);
        if (!string.IsNullOrEmpty(tooltipText))
        {
            _hoverText = tooltipText;
            ShowTooltip();
        }
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

    private string RequestTooltipText(long entityId, Point globalMousePos)
    {
        var evt = Scene.Events.Invoke(new TooltipRequestedEvent()
        {
            EntityId = entityId,
            Position = GetLocalMousePos(entityId, globalMousePos),
            GlobalPosition = globalMousePos,
            TooltipText = null
        });
        return evt.TooltipText ?? "";
    }

    private void StopHovering()
    {
        if (_hoveredProviderId != -1)
        {
            _hoveredProviderId = -1;
            _hoverTimer = 0;
        }
        DismissTooltip();
    }

    private void ShowTooltip()
    {
        if (TooltipVisible) return;
        _popupEntityId = CreateTooltip(_hoverText, _hoverMousePos);
    }
    
    private void DismissTooltip()
    {
        if (!TooltipVisible) return;
        Scene.Events.Invoke(new PopupDismissalRequested()
        {
            PopupEntityId = _popupEntityId
        });
        _popupEntityId = -1;
    }

    private long CreateTooltip(string text, Point mousePos)
    {
        var tooltipEntity = Scene.CreateEntity("Tooltip");
        tooltipEntity.AddComponent<Control>() = new Control
        {
            RequestLayout = true,
            MouseFilter = Control.MouseFilterMode.Ignore
        };
        tooltipEntity.AddComponent<Popup>() = new Popup()
        {
            DismissOnClick = Popup.PopupClickCondition.Outside
        };
        
        var buildEvt = Scene.Events.Invoke(new TooltipBuildRequestedEvent()
        {
            SourceEntityId = _hoveredProviderId,
            TooltipEntityId = tooltipEntity.EntityId,
            TooltipText = text,
            Position = GetLocalMousePos(_hoveredProviderId, _hoverMousePos),
            GlobalPosition = _hoverMousePos
        });
        var startPos = mousePos + buildEvt.Offset;
        tooltipEntity.GetComponent<Transform>().Position = startPos.ToVector2(); // TODO transform to viewport space

        return tooltipEntity.EntityId;
    }

    public override void OnUpdate()
    {
        if (_hoveredProviderId != -1)
        {
            var interfaceSys = Scene.Systems.Get<ControlInterfaceSystem>();
            if (interfaceSys != null)
            {
                if (interfaceSys.AnyMouseDown()) return;
            }
            
            _hoverTimer += Time.DeltaTime;

            if (_hoverTimer > TooltipDelaySec && !TooltipVisible)
            {
                AttemptShowTooltip();
            }
        }
        base.OnUpdate();
    }
}