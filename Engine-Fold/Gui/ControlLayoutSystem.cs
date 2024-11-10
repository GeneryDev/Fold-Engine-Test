using System;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui;

[GameSystem("fold:control.layout", ProcessingCycles.Render, true)]
public class ControlLayoutSystem : GameSystem
{
    private ComponentIterator<Control> _controls;

    public override void Initialize()
    {
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        _controls.Reset();
        while (_controls.Next())
        {
            ref var control = ref _controls.GetComponent();
            if (control.RequestLayout)
            {
                control.RequestLayout = false;
                Scene.Events.Invoke(new LayoutRequestedEvent(_controls.GetEntityId()));
            }
        }
    }

    public override void SubscribeToEvents()
    {
        this.Subscribe((ref LayoutRequestedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

            Layout(evt.EntityId);
        });
    }

    private void Layout(long entityId)
    {
        Console.WriteLine($"Performing layout on entity ID {entityId}");
        ref var transform = ref Scene.Components.GetComponent<Transform>(entityId);
        ref var control = ref Scene.Components.GetComponent<Control>(entityId);

        DeconstructParentBounds(ref transform, out var parentPosition, out var parentSize);
        var parentBegin = parentPosition;
        var parentEnd = parentPosition + parentSize;

        if (control.UseAnchors)
        {
            var anchorBegin = new Vector2(
                Mathf.Lerp(parentBegin.X, parentEnd.X, control.AnchorLeft),
                Mathf.Lerp(parentBegin.Y, parentEnd.Y, control.AnchorTop)
            );
            var anchorEnd = new Vector2(
                Mathf.Lerp(parentBegin.X, parentEnd.X, control.AnchorRight),
                Mathf.Lerp(parentBegin.Y, parentEnd.Y, control.AnchorBottom)
            );

            var ownBegin = anchorBegin + new Vector2(control.OffsetLeft, control.OffsetTop);
            var ownEnd = anchorEnd + new Vector2(control.OffsetRight, control.OffsetBottom);

            control.Position = ownBegin;
            control.Size = ownEnd - ownBegin;
        }
    }

    private void DeconstructParentBounds(ref Transform transform, out Vector2 parentPosition, out Vector2 parentSize)
    {
        if (transform is { HasParent: true, ParentId: var parentId } && Scene.Components.HasComponent<Control>(parentId))
        {
            ref var parentControl = ref Scene.Components.GetComponent<Control>(parentId);
            Scene.Events.Invoke(new LayoutRequestedEvent(parentId));

            parentPosition = parentControl.Position;
            parentSize = parentControl.Size;
        }
        else
        {
            // get size from rendering layer
            
            parentPosition = Vector2.Zero;
            parentSize = Scene.Core.RenderingUnit.WorldLayer.LayerSize.ToVector2();
        }
    }
}