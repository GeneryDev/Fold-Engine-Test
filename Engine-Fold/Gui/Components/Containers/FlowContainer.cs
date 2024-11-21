using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Containers;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components.Containers
{
    [Component("fold:control.flow_container", traits: [typeof(Control), typeof(Container)])]
    public struct FlowContainer
    {
        public bool Vertical;
        public Alignment Alignment;

        public float HSeparation;
        public float VSeparation;
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlLayoutSystem
    {
        private void SubscribeToFlowContainerEvents()
        {
            this.Subscribe((ref LayoutRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<FlowContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var transform = ref Scene.Components.GetComponent<Transform>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var flow = ref Scene.Components.GetComponent<FlowContainer>(evt.EntityId);
                
                LayoutFlowContainer(evt.ViewportId, ref transform, ref control, ref flow);
            });
            this.Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<FlowContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var transform = ref Scene.Components.GetComponent<Transform>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var flow = ref Scene.Components.GetComponent<FlowContainer>(evt.EntityId);
                
                ComputeFlowContainerSize(evt.ViewportId, ref transform, ref control, ref flow);
            });
        }

        private void LayoutFlowContainer(long viewportId, ref Transform transform, ref Control control, ref FlowContainer flow)
        {
            bool vertical = flow.Vertical;

            float offsetMain = 0;
            float offsetSec = 0;

            float containerSizeMain = vertical ? control.Size.Y : control.Size.X;
            float separationMain = vertical ? flow.VSeparation : flow.HSeparation;
            float separationSec = vertical ? flow.HSeparation : flow.VSeparation;

            float lastRowSecSize = 0;
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
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    childControl.Size = childControl.EffectiveMinimumSize;

                    float sizeMain = vertical ? childControl.Size.Y : childControl.Size.X;
                    float sizeSec = vertical ? childControl.Size.X : childControl.Size.Y;

                    if (offsetMain + sizeMain > containerSizeMain)
                    {
                        overflowed = true;
                        remainingRowGap = containerSizeMain - (offsetMain == 0 ? 0 : offsetMain - separationMain);
                        lastRowSecSize = maxSecSize;
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
                    AlignFlowRow(rowStartId, prevChildId, flow, remainingRowGap, lastRowSecSize);
                    rowStartId = childId;
                }

                prevChildId = childId;
                childId = childTransform.NextSiblingId;
                if (childId == -1)
                {
                    //reached end
                    remainingRowGap = containerSizeMain - (offsetMain == 0 ? 0 : offsetMain - separationMain);
                    lastRowSecSize = maxSecSize;
                    AlignFlowRow(rowStartId, prevChildId, flow, remainingRowGap, lastRowSecSize);
                }
            }
            
            LayoutChildren(viewportId, ref transform);
        }

        private void AlignFlowRow(long rowStartId, long rowEndId, FlowContainer container, float remainingGap, float rowSecSize)
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
                    ref var rowElementControl = ref Scene.Components.GetComponent<Control>(rowElementId);
                    if (container.Vertical)
                    {
                        rowElementTransform.Position += Vector2.UnitY * rowOffsetMain;
                        rowElementControl.Size = rowElementControl.Size with { X = rowSecSize };
                    }
                    else
                    {
                        rowElementTransform.Position += Vector2.UnitX * rowOffsetMain;
                        rowElementControl.Size = rowElementControl.Size with { Y = rowSecSize };
                    }
                }

                if (rowElementId == rowEndId) break;
            
                rowElementId = rowElementTransform.NextSiblingId;
            }
        }

        private void ComputeFlowContainerSize(long viewportId, ref Transform transform, ref Control control,
            ref FlowContainer flow)
        {
            bool vertical = flow.Vertical;
            var minimumMainSize = 0f;
            var minimumSecSize = 0f;
            float separationSec = vertical ? flow.HSeparation : flow.VSeparation;
            
            long childId = transform.FirstChildId;
            bool isFirstChildControl = true;
            while (childId != -1)
            {
                ref var childTransform = ref Scene.Components.GetComponent<Transform>(childId);
                
                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    float sizeMain = vertical ? childControl.EffectiveMinimumSize.Y : childControl.EffectiveMinimumSize.X;
                    float sizeSec = vertical ? childControl.EffectiveMinimumSize.X : childControl.EffectiveMinimumSize.Y;
                    
                    minimumMainSize = Math.Max(minimumMainSize, sizeMain);
                    minimumSecSize += sizeSec;
                    if (!isFirstChildControl)
                    {
                        minimumSecSize += separationSec;
                    }

                    isFirstChildControl = false;
                }
                childId = childTransform.NextSiblingId;
            }

            control.ComputedMinimumSize = vertical
                ? new Vector2(minimumSecSize, minimumMainSize)
                : new Vector2(minimumMainSize, minimumSecSize);
        }
    }
}