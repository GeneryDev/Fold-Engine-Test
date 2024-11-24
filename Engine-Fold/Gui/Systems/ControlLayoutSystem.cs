using System;
using FoldEngine.Components;
using FoldEngine.Editor.Transactions;
using FoldEngine.Events;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Containers;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.layout", ProcessingCycles.Render, true)]
public partial class ControlLayoutSystem : GameSystem
{
    private ComponentIterator<Viewport> _viewports;
    private ComponentIterator<Control> _controls;

    private long _mainViewportId = -1;

    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        _mainViewportId = -1;
        
        _viewports.Reset();
        while (_viewports.Next())
        {
            _mainViewportId = _viewports.GetEntityId();
            break;
        }

        if (_mainViewportId == -1) return;
        
        _controls.Reset();
        while (_controls.Next())
        {
            ref var control = ref _controls.GetComponent();
            if (control.RequestLayout)
            {
                control.RequestLayout = false;
                var requestTarget = _controls.GetEntityId();
                
                ref var transform = ref Scene.Components.GetComponent<Transform>(requestTarget);
                if (transform.HasParent && Scene.Components.HasComponent<Control>(transform.ParentId))
                {
                    //request layout for parent instead of this control
                    requestTarget = transform.ParentId;
                }
                
                Scene.Events.Invoke(new MinimumSizeRequestedEvent(requestTarget, _mainViewportId));
                Scene.Events.Invoke(new LayoutRequestedEvent(requestTarget, _mainViewportId));
            }
        }
    }

    public override void SubscribeToEvents()
    {
        SubscribeToAnchoredControlEvents();
        SubscribeToFlowContainerEvents();
        SubscribeToBorderContainerEvents();
        this.Subscribe((ref LayoutRequestedEvent evt) =>
        {
            if (evt.ViewportId == -1) return;
            if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;
            ref var transform = ref Scene.Components.GetComponent<Transform>(evt.EntityId);

            if (!Scene.Components.HasTrait<Container>(evt.EntityId))
            {
                LayoutFreeContainer(evt.ViewportId, ref transform);
            }
        });
        this.Subscribe((ref InspectorEditedComponentEvent evt) =>
        {
            if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;
            if (!(evt.ComponentType == typeof(Control) || Scene.Core.RegistryUnit.Components.HasTrait(evt.ComponentType, typeof(Control)))) return;
            ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
            control.RequestLayout = true;
        });
        // TODO request layout for top-level controls when window size changes
        Subscribe((ref WindowSizeChangedEvent evt) =>
        {
            
        });
    }

    private void DeconstructParentBounds(long viewportId, ref Transform transform, out Vector2 parentPosition, out Vector2 parentSize)
    {
        if (transform is { HasParent: true, ParentId: var parentId } && Scene.Components.HasComponent<Control>(parentId))
        {
            ref var parentTransform = ref Scene.Components.GetComponent<Transform>(parentId);
            ref var parentControl = ref Scene.Components.GetComponent<Control>(parentId);

            parentPosition = parentTransform.Position;
            parentSize = parentControl.Size;
        }
        else
        {
            parentPosition = Vector2.Zero;
            
            // get size from rendering layer
            if (viewportId != -1 && Scene.Components.HasComponent<Viewport>(viewportId))
            {
                var viewport = Scene.Components.GetComponent<Viewport>(viewportId);
                var layer = viewport.GetLayer(Scene.Core.RenderingUnit);
                parentSize = layer?.LayerSize.ToVector2() ?? Vector2.One;
            }
            else
            {
                parentSize = Scene.Core.RenderingUnit.WindowLayer.LayerSize.ToVector2();
            }
        }
    }

    private void LayoutFreeContainer(long viewportId, ref Transform transform)
    {
        long childId = transform.FirstChildId;
        while (childId != -1)
        {
            var childTransform = Scene.Components.GetComponent<Transform>(childId);

            if (Scene.Components.HasComponent<Control>(childId))
            {
                Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));
                Scene.Events.Invoke(new LayoutRequestedEvent(childId, viewportId));
            }

            childId = childTransform.NextSiblingId;
        }
    }

    private void LayoutChildren(long viewportId, ref Transform transform)
    {
        long childId = transform.FirstChildId;
        while (childId != -1)
        {
            var childTransform = Scene.Components.GetComponent<Transform>(childId);

            if (Scene.Components.HasComponent<Control>(childId))
            {
                Scene.Events.Invoke(new LayoutRequestedEvent(childId, viewportId));
            }

            childId = childTransform.NextSiblingId;
        }
    }
}