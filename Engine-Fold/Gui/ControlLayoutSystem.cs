﻿using System;
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

        if (Scene.Components.HasComponent<AnchoredControl>(entityId))
        {
            LayoutAnchoredControl(ref transform, ref control, ref Scene.Components.GetComponent<AnchoredControl>(entityId));
        }
        if (Scene.Components.HasComponent<FreeContainer>(entityId))
        {
            LayoutFreeContainer(ref transform);
        }
        if (Scene.Components.HasComponent<FlowContainer>(entityId))
        {
            LayoutBoxContainer(ref transform, ref control, ref Scene.Components.GetComponent<FlowContainer>(entityId));
        }
    }

    private void LayoutAnchoredControl(ref Transform transform, ref Control control, ref AnchoredControl anchored)
    {
        DeconstructParentBounds(ref transform, out _, out var parentSize);
        var parentBegin = Vector2.Zero;
        var parentEnd = Vector2.Zero + parentSize;

        var anchorBegin = new Vector2(
            Mathf.Lerp(parentBegin.X, parentEnd.X, anchored.AnchorLeft),
            Mathf.Lerp(parentBegin.Y, parentEnd.Y, anchored.AnchorTop)
        );
        var anchorEnd = new Vector2(
            Mathf.Lerp(parentBegin.X, parentEnd.X, anchored.AnchorRight),
            Mathf.Lerp(parentBegin.Y, parentEnd.Y, anchored.AnchorBottom)
        );

        var ownBegin = anchorBegin + new Vector2(anchored.OffsetLeft, anchored.OffsetTop);
        var ownEnd = anchorEnd + new Vector2(anchored.OffsetRight, anchored.OffsetBottom);

        var minimumSize = control.EffectiveMinimumSize;
        if(minimumSize != Vector2.Zero) {
            //Apply minimum size
            var growAmount = minimumSize - (ownEnd - ownBegin);
            growAmount = new Vector2(
                Math.Max(0, growAmount.X),
                Math.Max(0, growAmount.Y)
                );

            if (growAmount.X > 0)
            {
                var growRatio = anchored.GrowHorizontal switch
                {
                    AnchoredControl.GrowDirection.Begin => (-1f, 0f),
                    AnchoredControl.GrowDirection.Both => (-0.5f, 0.5f),
                    AnchoredControl.GrowDirection.End => (0f, 1f),
                    _ => (0f, 0f)
                }; 
                ownBegin.X += growAmount.X * growRatio.Item1;
                ownEnd.X += growAmount.X * growRatio.Item2;
            }

            if (growAmount.Y > 0)
            {
                var growRatio = anchored.GrowVertical switch
                {
                    AnchoredControl.GrowDirection.Begin => (-1f, 0f),
                    AnchoredControl.GrowDirection.Both => (-0.5f, 0.5f),
                    AnchoredControl.GrowDirection.End => (0f, 1f),
                    _ => (0f, 0f)
                }; 
                ownBegin.Y += growAmount.Y * growRatio.Item1;
                ownEnd.Y += growAmount.Y * growRatio.Item2;
            }

        }

        transform.LocalPosition = ownBegin;
        control.Size = ownEnd - ownBegin;
        
        LayoutChildren(ref transform);
    }

    private void LayoutFreeContainer(ref Transform transform)
    {
        long childId = transform.FirstChildId;
        while (childId != -1)
        {
            var childTransform = Scene.Components.GetComponent<Transform>(childId);

            if (Scene.Components.HasComponent<Control>(childId))
            {
                Scene.Events.Invoke(new LayoutRequestedEvent(childId));
            }

            childId = childTransform.NextSiblingId;
        }
    }
    
    private void LayoutBoxContainer(ref Transform transform, ref Control control, ref FlowContainer flow)
    {
        bool vertical = flow.Vertical;

        float offsetMain = 0;
        float offsetSec = 0;

        float containerSizeMain = vertical ? control.Size.Y : control.Size.X;
        float separationMain = vertical ? flow.VSeparation : flow.HSeparation;
        float separationSec = vertical ? flow.HSeparation : flow.VSeparation;

        float maxSecSize = 0;

        long childId = transform.FirstChildId;
        long rowStartId = childId;
        long prevChildId = -1;
        while (childId != -1)
        {
            ref var childTransform = ref Scene.Components.GetComponent<Transform>(childId);

            var overflowed = false;
            float remainingRowGap = 0;

            if (Scene.Components.HasComponent<Control>(childId))
            {
                ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                Scene.Events.Invoke(new LayoutRequestedEvent(childId));

                float sizeMain = vertical ? childControl.Size.Y : childControl.Size.X;
                float sizeSec = vertical ? childControl.Size.X : childControl.Size.Y;

                if (offsetMain + sizeMain > containerSizeMain)
                {
                    overflowed = true;
                    remainingRowGap = containerSizeMain - (offsetMain == 0 ? 0 : offsetMain - separationMain);
                    // wrap
                    offsetSec += maxSecSize + separationSec;
                    offsetMain = 0;
                    maxSecSize = 0;
                }
                
                childTransform.Position = vertical
                    ? new Vector2(offsetSec, offsetMain)
                    : new Vector2(offsetMain, offsetSec);

                offsetMain += sizeMain + separationMain;
                maxSecSize = Math.Max(maxSecSize, sizeSec);
            }
            
            // End of row. Iterate through all of the prior
            if (overflowed)
            {
                AlignFlowRow(rowStartId, prevChildId, flow, remainingRowGap);
                rowStartId = childId;
            }

            prevChildId = childId;
            childId = childTransform.NextSiblingId;
            if (childId == -1)
            {
                //reached end
                remainingRowGap = containerSizeMain - (offsetMain == 0 ? 0 : offsetMain - separationMain);
                AlignFlowRow(rowStartId, prevChildId, flow, remainingRowGap);
            }
        }
    }

    private void AlignFlowRow(long rowStartId, long rowEndId, FlowContainer container, float remainingGap)
    {
        // Process previous row
        float rowOffsetMain = remainingGap * (container.Alignment switch
        {
            Alignment.Begin => 0.0f,
            Alignment.Center => 0.5f,
            Alignment.End => 1.0f,
            _ => 0
        });
        Console.WriteLine("row offset main: " + rowOffsetMain);

        long rowElementId = rowStartId;
        while (rowElementId != -1)
        {
            ref var rowElementTransform = ref Scene.Components.GetComponent<Transform>(rowElementId);

            if (Scene.Components.HasComponent<Control>(rowElementId))
            {
                rowElementTransform.Position += (container.Vertical ? Vector2.UnitY : Vector2.UnitX) * rowOffsetMain;
            }

            if (rowElementId == rowEndId) break;
            
            rowElementId = rowElementTransform.NextSiblingId;
        }
    }

    private void DeconstructParentBounds(ref Transform transform, out Vector2 parentPosition, out Vector2 parentSize)
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
            // get size from rendering layer
            
            parentPosition = Vector2.Zero;
            parentSize = Scene.Core.RenderingUnit.WorldLayer.LayerSize.ToVector2();
        }
    }

    private void LayoutChildren(ref Transform transform)
    {
        long childId = transform.FirstChildId;
        while (childId != -1)
        {
            var childTransform = Scene.Components.GetComponent<Transform>(childId);

            if (Scene.Components.HasComponent<Control>(childId))
            {
                Scene.Events.Invoke(new LayoutRequestedEvent(childId));
            }

            childId = childTransform.NextSiblingId;
        }
    }
}