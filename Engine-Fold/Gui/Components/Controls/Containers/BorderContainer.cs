using System;
using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components.Controls.Containers
{
    [Component("fold:control.border_container", traits: [typeof(Control), typeof(Container)])]
    [ComponentInitializer(typeof(BorderContainer))]
    public struct BorderContainer
    {
        [EntityId] public long NorthPanelId = -1;
        [EntityId] public long WestPanelId = -1;
        [EntityId] public long EastPanelId = -1;
        [EntityId] public long SouthPanelId = -1;
        
        public CornerBias CornerBiasNorthWest = CornerBias.Horizontal;
        public CornerBias CornerBiasNorthEast = CornerBias.Horizontal;
        public CornerBias CornerBiasSouthWest = CornerBias.Horizontal;
        public CornerBias CornerBiasSouthEast = CornerBias.Horizontal;

        public BorderContainer() {}

        public enum CornerBias
        {
            Horizontal,
            Vertical
        }
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlLayoutSystem
    {
        private void SubscribeToBorderContainerEvents()
        {
            this.Subscribe((ref LayoutRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<BorderContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<BorderContainer>(evt.EntityId);
                
                LayoutBorderContainer(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
            this.Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.ViewportId == -1) return;
                if (!Scene.Components.HasComponent<BorderContainer>(evt.EntityId)) return;
                if (!Scene.Components.HasComponent<Control>(evt.EntityId)) return;

                ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
                ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);
                ref var container = ref Scene.Components.GetComponent<BorderContainer>(evt.EntityId);
                
                ComputeBorderContainerSize(evt.ViewportId, ref hierarchical, ref control, ref container);
            });
        }

        private void LayoutBorderContainer(long viewportId, ref Hierarchical hierarchical, ref Control control, ref BorderContainer container)
        {
            ComputeBorderContainerSizes(viewportId,
                ref hierarchical,
                ref container,
                out float sizeNorth,
                out float sizeWest,
                out float sizeEast,
                out float sizeSouth,
                out _);

            var centerBounds = new Rect2(sizeWest, sizeNorth, control.Size.X - (sizeEast + sizeWest),
                control.Size.Y - (sizeNorth + sizeSouth));
            
            // Iterate over children again and set their appropriate bounds
            foreach(long childId in hierarchical.GetChildren())
            {
                ref var childTransform = ref Scene.Components.GetComponent<Transform>(childId);

                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    childControl.Size = childControl.EffectiveMinimumSize;

                    Rect2 childBounds = centerBounds;

                    if (container.NorthPanelId == childId)
                    {
                        childBounds = new Rect2(0, 0, control.Size.X, sizeNorth);
                        if (container.CornerBiasNorthWest == BorderContainer.CornerBias.Vertical)
                        {
                            childBounds.X += sizeWest;
                            childBounds.Width -= sizeWest;
                        }

                        if (container.CornerBiasNorthEast == BorderContainer.CornerBias.Vertical) childBounds.Width -= sizeEast;
                    }

                    if (container.WestPanelId == childId)
                    {
                        childBounds = new Rect2(0, 0, sizeWest, control.Size.Y);
                        if (container.CornerBiasNorthWest == BorderContainer.CornerBias.Horizontal)
                        {
                            childBounds.Y += sizeNorth;
                            childBounds.Height -= sizeNorth;
                        }

                        if (container.CornerBiasSouthWest == BorderContainer.CornerBias.Horizontal) childBounds.Height -= sizeSouth;
                    }

                    if (container.EastPanelId == childId)
                    {
                        childBounds = new Rect2(control.Size.X - sizeEast, 0, sizeEast, control.Size.Y);
                        if (container.CornerBiasNorthEast == BorderContainer.CornerBias.Horizontal)
                        {
                            childBounds.Y += sizeNorth;
                            childBounds.Height -= sizeNorth;
                        }

                        if (container.CornerBiasSouthEast == BorderContainer.CornerBias.Horizontal) childBounds.Height -= sizeSouth;
                    }

                    if (container.SouthPanelId == childId)
                    {
                        childBounds = new Rect2(0, control.Size.Y - sizeSouth, control.Size.X, sizeSouth);
                        if (container.CornerBiasSouthWest == BorderContainer.CornerBias.Vertical)
                        {
                            childBounds.X += sizeWest;
                            childBounds.Width -= sizeWest;
                        }

                        if (container.CornerBiasSouthEast == BorderContainer.CornerBias.Vertical) childBounds.Width -= sizeEast;
                    }

                    childTransform.LocalPosition = childBounds.Position;
                    childControl.Size = childBounds.Size;
                }
            }
            
            LayoutChildren(viewportId, ref hierarchical);
        }

        private void ComputeBorderContainerSizes(long viewportId, ref Hierarchical hierarchical, ref BorderContainer container, out float sizeNorth, out float sizeWest, out float sizeEast, out float sizeSouth, out Vector2 sizeCenter)
        {
            sizeNorth = 0;
            sizeSouth = 0;
            sizeWest = 0;
            sizeEast = 0;
            sizeCenter = Vector2.Zero;
            
            // First, request the minimum sizes of all children
            
            foreach(long childId in hierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Control>(childId))
                {
                    ref var childControl = ref Scene.Components.GetComponent<Control>(childId);
                    Scene.Events.Invoke(new MinimumSizeRequestedEvent(childId, viewportId));

                    var childSize = childControl.EffectiveMinimumSize;
                    var isBorderPanel = false;

                    if (container.NorthPanelId == childId)
                    {
                        sizeNorth = childSize.Y;
                        sizeCenter = new Vector2(
                            Math.Max(sizeCenter.X, childSize.X),
                            sizeCenter.Y
                        );
                        isBorderPanel = true;
                    }

                    if (container.WestPanelId == childId)
                    {
                        sizeWest = childSize.X;
                        sizeCenter = new Vector2(
                            sizeCenter.X,
                            Math.Max(sizeCenter.Y, childSize.Y)
                        );
                        isBorderPanel = true;
                    }

                    if (container.EastPanelId == childId)
                    {
                        sizeEast = childSize.X;
                        sizeCenter = new Vector2(
                            sizeCenter.X,
                            Math.Max(sizeCenter.Y, childSize.Y)
                        );
                        isBorderPanel = true;
                    }

                    if (container.SouthPanelId == childId)
                    {
                        sizeSouth = childSize.Y;
                        sizeCenter = new Vector2(
                            Math.Max(sizeCenter.X, childSize.X),
                            sizeCenter.Y
                        );
                        isBorderPanel = true;
                    }

                    if (!isBorderPanel)
                    {
                        sizeCenter = new Vector2(
                            Math.Max(sizeCenter.X, childSize.X),
                            Math.Max(sizeCenter.Y, childSize.Y)
                            );
                    }
                }
            }
        }

        private void ComputeBorderContainerSize(long viewportId, ref Hierarchical hierarchical, ref Control control,
            ref BorderContainer container)
        {
            ComputeBorderContainerSizes(viewportId,
                ref hierarchical,
                ref container,
                out float sizeNorth,
                out float sizeWest,
                out float sizeEast,
                out float sizeSouth,
                out var sizeCenter);

            // TODO sizeCenter computed values are incorrect since they don't account for panels being extended into corners.
            // Please fix
            control.ComputedMinimumSize =
                new Vector2(sizeWest + sizeCenter.X + sizeEast, sizeNorth + sizeCenter.Y + sizeSouth);
        }
    }
}