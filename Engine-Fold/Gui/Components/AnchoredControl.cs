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
    [ComponentInitializer(typeof(AnchoredControl), nameof(InitializeComponent))]
    public struct AnchoredControl
    {
        public float AnchorLeft;
        public float AnchorTop;
        public float AnchorRight;
        public float AnchorBottom;

        public float OffsetLeft;
        public float OffsetTop;
        public float OffsetRight;
        public float OffsetBottom;
    
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
    
        /// <summary>
        ///     Returns an initialized anchored component with all its correct default values.
        /// </summary>
        public static AnchoredControl InitializeComponent(Scene scene, long entityId)
        {
            return new AnchoredControl();
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
        
            LayoutChildren(viewportId, ref hierarchical);
        }
    }
}