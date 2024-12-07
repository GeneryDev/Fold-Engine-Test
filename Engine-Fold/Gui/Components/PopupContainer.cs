using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components.Controls.Containers
{
    [Component("fold:control.popup_container", traits: [typeof(Control), typeof(Container)])]
    public struct PopupContainer
    {
        public Vector2 PopupPosition;
        public Vector2 Gap;
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlLayoutSystem
    {
        private void SubscribeToPopupContainerEvents()
        {
            this.Subscribe((ref LayoutRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<PopupContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<PopupContainer>(evt.EntityId);
                
                LayoutPopupContainer(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
            this.Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<PopupContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<PopupContainer>(evt.EntityId);
                
                ComputePopupContainerSize(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
        }

        private void LayoutPopupContainer(long viewportId, ref Hierarchical hierarchical, ref Control control, ref PopupContainer container)
        {
            var containerSize = control.Size;
            var contentSize = new Vector2(0, 0);

            // Get the maximum of all the children's minimum sizes 
            foreach(long childId in hierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    contentSize = new Vector2(
                        Math.Max(contentSize.X, childControl.EffectiveMinimumSize.X),
                        Math.Max(contentSize.Y, childControl.EffectiveMinimumSize.Y)
                    );
                }
            }

            // Compute different options for where to place the top left corner of the children
            var contentPosPositive = container.PopupPosition + container.Gap;
            var contentPosFloored = Vector2.Zero;
            var contentPosNegative = container.PopupPosition - contentSize - container.Gap;

            // Decide where to place the top left corner of the children
            var contentPos = contentPosPositive;
            // If the children go off the screen to the bottom/right, use the negative variant of the computed corners
            if (contentPos.X + contentSize.X > containerSize.X)
            {
                contentPos.X = contentPosNegative.X;
            }
            if (contentPos.Y + contentSize.Y > containerSize.Y)
            {
                contentPos.Y = contentPosNegative.Y;
            }
            // If the children still go off screen to the top/left, clamp to zero
            if (contentPos.X < 0)
            {
                contentPos.X = contentPosFloored.X;
            }
            if (contentPos.Y < 0)
            {
                contentPos.Y = contentPosFloored.Y;
            }
            // If the children still go off screen, tough luck (TODO fix here if it becomes a problem)
            
            
            // Apply the computed position and size to children
            foreach(long childId in hierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Transform>(childId) && Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childTransform = ref Scene.Components.GetComponent<Transform>(childId);
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);

                    childTransform.LocalPosition = contentPos;
                    childControl.Size = contentSize;
                }
            }
            
            LayoutChildren(viewportId, ref hierarchical);
        }

        private void ComputePopupContainerSize(long viewportId, ref Hierarchical hierarchical, ref Control control,
            ref PopupContainer container)
        {
            var minSize = new Vector2(0, 0);

            foreach(long childId in hierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    minSize = new Vector2(
                        Math.Max(minSize.X, childControl.EffectiveMinimumSize.X),
                        Math.Max(minSize.Y, childControl.EffectiveMinimumSize.Y)
                        );
                }
            }
            
            minSize.X += Math.Max(container.Gap.X, 0);
            minSize.Y += Math.Max(container.Gap.Y, 0);

            control.ComputedMinimumSize = minSize;
        }
    }
}