using System;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components
{
    [Component("fold:control.anchored", traits: [typeof(Control)])]
    [ComponentInitializer(typeof(AnchoredControl))]
    public struct AnchoredControl
    {
        public LRTB Anchor;
        public LRTB Offset;
    
        public GrowDirection GrowHorizontal;
        public GrowDirection GrowVertical;

        public enum GrowDirection
        {
            Begin,
            End,
            Both
        }

        public AnchoredControl()
        {
            GrowHorizontal = GrowVertical = GrowDirection.Both;
        }

        public static class Presets
        {
            public static readonly LRTB TopLeft = new(Left: 0, Right: 0, Top: 0, Bottom: 0);
            public static readonly LRTB TopRight = new(Left: 1, Right: 1, Top: 0, Bottom: 0);
            public static readonly LRTB BottomLeft = new(Left: 0, Right: 0, Top: 1, Bottom: 1);
            public static readonly LRTB BottomRight = new(Left: 1, Right: 1, Top: 1, Bottom: 1);
            
            public static readonly LRTB CenterLeft = new(Left: 0, Right: 0, Top: 0.5f, Bottom: 0.5f);
            public static readonly LRTB CenterRight = new(Left: 1, Right: 1, Top: 0.5f, Bottom: 0.5f);
            public static readonly LRTB CenterTop = new(Left: 0.5f, Right: 0.5f, Top: 0, Bottom: 0);
            public static readonly LRTB CenterBottom = new(Left: 0.5f, Right: 0.5f, Top: 1, Bottom: 1);
            
            public static readonly LRTB LeftWide = new(Left: 0, Right: 0, Top: 0, Bottom: 1);
            public static readonly LRTB RightWide = new(Left: 1, Right: 1, Top: 0, Bottom: 1);
            public static readonly LRTB TopWide = new(Left: 0, Right: 1, Top: 0, Bottom: 0);
            public static readonly LRTB BottomWide = new(Left: 0, Right: 1, Top: 1, Bottom: 1);
            
            public static readonly LRTB Center = new(Left: 0.5f, Right: 0.5f, Top: 0.5f, Bottom: 0.5f);
            public static readonly LRTB HCenterWide = new(Left: 0, Right: 1, Top: 0.5f, Bottom: 0.5f);
            public static readonly LRTB VCenterWide = new(Left: 0.5f, Right: 0.5f, Top: 0, Bottom: 1);
            
            public static readonly LRTB FullRect = new(Left: 0, Right: 1, Top: 0, Bottom: 1);
        }
    }
}


namespace FoldEngine.Gui.Systems
{
    public partial class ControlLayoutSystem
    {
        private void SubscribeToAnchoredControlEvents()
        {
            this.Subscribe((ref LayoutRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<AnchoredControl>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var transform = ref Scene.Components.GetComponent<Transform>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var anchored = ref Scene.Components.GetComponent<AnchoredControl>(evt.EntityId);
                
                LayoutAnchoredControl(evt.ViewportId, ref hierarchical, ref transform, ref control, ref anchored);
            });
            this.Subscribe(Scene.Core.Events, (ref WindowSizeChangedEvent evt) =>
            {
                _anchoredControls.Reset();
                while (_anchoredControls.Next())
                {
                    if (!_anchoredControls.HasCoComponent<Control>()) continue;
                    ref var hierarchical = ref _anchoredControls.GetCoComponent<Hierarchical>();
                    // If no parent, or parent is not a control, update the layout
                    if (!hierarchical.HasParent ||
                        !Scene.Components.HasComponent<Control>(hierarchical.Parent.EntityId))
                    {
                        _anchoredControls.GetCoComponent<Control>().RequestLayout = true;
                    }
                }
            });
        }

        private void LayoutAnchoredControl(long viewportId, ref Hierarchical hierarchical, ref Transform transform, ref Control control, ref AnchoredControl anchored)
        {
            DeconstructParentBounds(viewportId, ref hierarchical, out _, out var parentSize);
            var parentBegin = Vector2.Zero;
            var parentEnd = Vector2.Zero + parentSize;

            var anchorBegin = new Vector2(
                Mathf.Lerp(parentBegin.X, parentEnd.X, anchored.Anchor.Left),
                Mathf.Lerp(parentBegin.Y, parentEnd.Y, anchored.Anchor.Top)
            );
            var anchorEnd = new Vector2(
                Mathf.Lerp(parentBegin.X, parentEnd.X, anchored.Anchor.Right),
                Mathf.Lerp(parentBegin.Y, parentEnd.Y, anchored.Anchor.Bottom)
            );

            var ownBegin = anchorBegin + new Vector2(anchored.Offset.Left, anchored.Offset.Top);
            var ownEnd = anchorEnd + new Vector2(anchored.Offset.Right, anchored.Offset.Bottom);

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
        
            LayoutChildren(viewportId, ref hierarchical);
        }
    }
}