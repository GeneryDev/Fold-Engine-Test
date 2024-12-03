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
    [Component("fold:control.stack_container", traits: [typeof(Control), typeof(Container)])]
    public struct StackContainer
    {
        public bool Vertical;
        public Alignment Alignment;

        public float Separation;
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlLayoutSystem
    {
        private void SubscribeToStackContainerEvents()
        {
            this.Subscribe((ref LayoutRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<StackContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<StackContainer>(evt.EntityId);
                
                LayoutStackContainer(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
            this.Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<StackContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<StackContainer>(evt.EntityId);
                
                ComputeStackContainerSize(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
        }

        private void LayoutStackContainer(long viewportId, ref Hierarchical hierarchical, ref Control control, ref StackContainer container)
        {
            bool vertical = container.Vertical;

            float offsetMain = 0;
            float offsetSec = 0;

            float containerSizeMain = vertical ? control.Size.Y : control.Size.X;
            float containerSizeSec = vertical ? control.Size.X : control.Size.Y;
            float separationMain = container.Separation;

            float maxSecSize = 0;

            long childId = hierarchical.FirstChildId;
            while (childId != -1)
            {
                ref var childHierarchical = ref Scene.Components.GetComponent<Hierarchical>(childId);
                ref var childTransform = ref Scene.Components.GetComponent<Transform>(childId);

                if (Scene.Components.HasComponent<Control>(childId) && childHierarchical.IsActiveInHierarchy())
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    childControl.Size = childControl.EffectiveMinimumSize;

                    float sizeMain = vertical ? childControl.Size.Y : childControl.Size.X;
                    float sizeSec = vertical ? childControl.Size.X : childControl.Size.Y;

                    childTransform.LocalPosition = vertical
                        ? new Vector2(offsetSec, offsetMain)
                        : new Vector2(offsetMain, offsetSec);
                    if (vertical)
                        childControl.Size.X = containerSizeSec;
                    else
                        childControl.Size.Y = containerSizeSec;

                    offsetMain += sizeMain + separationMain;
                    maxSecSize = Math.Max(maxSecSize, sizeSec);
                }

                childId = childHierarchical.NextSiblingId;
            }
            
            LayoutChildren(viewportId, ref hierarchical);
        }

        private void ComputeStackContainerSize(long viewportId, ref Hierarchical hierarchical, ref Control control,
            ref StackContainer container)
        {
            bool vertical = container.Vertical;
            var minimumMainSize = 0f;
            var minimumSecSize = 0f;
            var separationMain = container.Separation;

            foreach(long childId in hierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    float sizeMain = vertical ? childControl.EffectiveMinimumSize.Y : childControl.EffectiveMinimumSize.X;
                    float sizeSec = vertical ? childControl.EffectiveMinimumSize.X : childControl.EffectiveMinimumSize.Y;

                    minimumMainSize += sizeMain + separationMain;
                    minimumSecSize = Math.Max(minimumSecSize, sizeSec);
                }
            }

            if (minimumMainSize > 0)
            {
                minimumMainSize -= separationMain;
            }

            control.ComputedMinimumSize = vertical
                ? new Vector2(minimumSecSize, minimumMainSize)
                : new Vector2(minimumMainSize, minimumSecSize);
        }
    }
}