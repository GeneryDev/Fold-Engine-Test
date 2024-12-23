using System;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components.Controls
{
    [Component("fold:control.resize_handle", traits: [typeof(Control), typeof(MouseFilterDefaultStop)])]
    [ComponentInitializer(typeof(ResizeHandleControl))]
    public struct ResizeHandleControl
    {
        public Vector2 ResizeDirection;
        [EntityId] public long EntityToResize = -1;
        public int VisibleThickness = 2;
        public Vector2 MinimumSize;
        
        public Color NormalColor => new Color(0, 0, 0, 0);
        public Color RolloverColor => new Color(140, 140, 145);
        public Color PressedColor => new Color(255, 255, 255);

        [DoNotSerialize] public bool Rollover;
        [DoNotSerialize] public bool Pressed;

        [DoNotSerialize] public Point DragStartPoint;
        [DoNotSerialize] public float DragStartValue = 0;
    
        public ResizeHandleControl()
        {
        }
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class StandardControlsSystem
    {
        private void RenderResizeHandle(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform,
            ref Control control, ref ResizeHandleControl handle)
        {
            var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
            var innerBounds = bounds;
            var normalizedDir = handle.ResizeDirection.Normalized();
            var lineBounds = new Rectangle(
                bounds.Location,
                new Point(
                    (int)Mathf.Lerp(innerBounds.Width, handle.VisibleThickness, Math.Abs(normalizedDir.X)),
                    (int)Mathf.Lerp(innerBounds.Height, handle.VisibleThickness, Math.Abs(normalizedDir.Y))
                )
            );
            lineBounds.X += (int)((innerBounds.Width - lineBounds.Width) * (normalizedDir.X+1)/2f);
            lineBounds.Y += (int)((innerBounds.Height - lineBounds.Height) * (normalizedDir.Y+1)/2f);
            
            Point offset = Point.Zero;

            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = handle.Pressed
                    ? handle.PressedColor
                    : handle.Rollover
                        ? handle.RolloverColor
                        : handle.NormalColor,
                DestinationRectangle = lineBounds.Translate(offset),
                Z = -control.ZOrder
            });
        }


        private void SubscribeToResizeHandleEvents()
        {
            Subscribe((ref MouseEnteredEvent evt) =>
            {
                if (Scene.Components.HasComponent<ResizeHandleControl>(evt.EntityId))
                {
                    Scene.Components.GetComponent<ResizeHandleControl>(evt.EntityId).Rollover = true;
                }
            });
            Subscribe((ref MouseExitedEvent evt) =>
            {
                if (Scene.Components.HasComponent<ResizeHandleControl>(evt.EntityId))
                {
                    Scene.Components.GetComponent<ResizeHandleControl>(evt.EntityId).Rollover = false;
                }
            });
            Subscribe((ref MouseButtonEvent evt) =>
            {
                if (evt.Consumed) return;
                if (evt.EntityId != -1 && Scene.Components.HasComponent<ResizeHandleControl>(evt.EntityId))
                {
                    ref var handle = ref Scene.Components.GetComponent<ResizeHandleControl>(evt.EntityId);
                    if (((int)MouseButtonMask.LeftButton & (1 << evt.Button)) == 0) return;
                    
                    switch (evt.Type)
                    {
                        case MouseButtonEventType.Pressed:
                        {
                            handle.Pressed = true;
                            handle.DragStartPoint = evt.Position;
                            
                            if (handle.EntityToResize != -1 && Scene.Components.HasComponent<Control>(handle.EntityToResize))
                            {
                                ref var controlToResize = ref Scene.Components.GetComponent<Control>(handle.EntityToResize);

                                handle.DragStartValue =
                                    Math.Abs(Vector2.Dot(handle.ResizeDirection.Normalized(), controlToResize.MinimumSize));
                            }
                            evt.Consume();

                            break;
                        }
                        case MouseButtonEventType.Released:
                        {
                            handle.Pressed = false;
                            evt.Consume();

                            break;
                        }
                    }
                }
            });
            Subscribe((ref MouseDraggedEvent evt) =>
            {
                if (!Scene.Components.HasComponent<ResizeHandleControl>(evt.EntityId)) return;
                ref var handle = ref Scene.Components.GetComponent<ResizeHandleControl>(evt.EntityId);
                var differenceFromStart = evt.Position - handle.DragStartPoint;
                float resizeAmount = Vector2.Dot(differenceFromStart.ToVector2(), handle.ResizeDirection);

                if (handle.EntityToResize != -1 && Scene.Components.HasComponent<Control>(handle.EntityToResize))
                {
                    ref var controlToResize = ref Scene.Components.GetComponent<Control>(handle.EntityToResize);

                    var perpendicularDir = new Vector2(handle.ResizeDirection.Y, -handle.ResizeDirection.X).Normalized();
                    var absResizeDirection = new Vector2(Math.Abs(handle.ResizeDirection.X), Math.Abs(handle.ResizeDirection.Y)).Normalized();

                    var newMinimumSize = absResizeDirection * (handle.DragStartValue + resizeAmount) +
                                         perpendicularDir * Vector2.Dot(perpendicularDir, controlToResize.MinimumSize);
                    newMinimumSize.X = Math.Max(newMinimumSize.X, handle.MinimumSize.X);
                    newMinimumSize.Y = Math.Max(newMinimumSize.Y, handle.MinimumSize.Y);
                    controlToResize.MinimumSize = newMinimumSize;
                    controlToResize.RequestLayout = true;
                }
            });
        }
    }
}