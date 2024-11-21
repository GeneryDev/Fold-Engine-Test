using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components
{
    [Component("fold:control.button", traits: [typeof(Control), typeof(MousePickable)])]
    [ComponentInitializer(typeof(ButtonControl), nameof(InitializeComponent))]
    public struct ButtonControl
    {
        public string Text;
        public int FontSize;
        public Color Color;
        public Alignment Alignment;
    
        public ResourceIdentifier Icon;
        public Color IconColor;
        public bool FitIcon;
        public float IconMaxWidth;
        public float IconTextSeparation;
    
        public bool KeepPressedOutside;
        public MouseActionMode ActionMode;
        public MouseButtonMask ButtonMask;

        [DoNotSerialize] [HideInInspector] public RenderedText RenderedText;
    
        public Color NormalColor => new Color(37, 37, 38);
        public Color RolloverColor => Color.CornflowerBlue;
        public Color PressedColor => new Color(63, 63, 70);
        [DoNotSerialize] public bool Rollover;
        [DoNotSerialize] public bool Pressed;
    
        public ButtonControl()
        {
            Text = "";
            FontSize = 14;
            Color = Color.White;
            Alignment = Alignment.Center;
            IconColor = Color.White;
            IconTextSeparation = 8;
            ActionMode = MouseActionMode.Release;
            ButtonMask = MouseButtonMask.LeftButton;
        }
    
        /// <summary>
        ///     Returns an initialized button component with all its correct default values.
        /// </summary>
        public static ButtonControl InitializeComponent(Scene scene, long entityId)
        {
            return new ButtonControl();
        }

        public bool UpdateRenderedText(IRenderingUnit renderer)
        {
            if (RenderedText.HasValue && RenderedText.Text == Text && RenderedText.Size == FontSize)
            {
                // already up to date
                return false;
            }
            renderer.Fonts["default"].RenderString(Text, out RenderedText, FontSize);
            return true;
        }
    }

    [Flags]
    public enum MouseButtonMask
    {
        LeftButton = 1 << MouseButtonEvent.LeftButton,
        MiddleButton = 1 << MouseButtonEvent.MiddleButton,
        RightButton = 1 << MouseButtonEvent.RightButton
    }
    public enum MouseActionMode
    {
        Press,
        Release
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlRenderer
    {
        private void RenderButton(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform,
            ref Control control, ref ButtonControl button)
        {
            var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
            var innerBounds = bounds;
            Point offset = Point.Zero;

            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = button.Pressed && (button.KeepPressedOutside || button.Rollover)
                    ? button.PressedColor
                    : button.Rollover
                        ? button.RolloverColor
                        : button.NormalColor,
                DestinationRectangle = innerBounds.Translate(offset),
                Z = -control.ZOrder
            });

            button.UpdateRenderedText(renderer);
            ref RenderedText renderedText = ref button.RenderedText;
            if (!renderedText.HasValue) return;

            float textWidth = renderedText.Width;

            float totalWidth = textWidth;
            
            var icon = Scene.Resources.Get<Texture>(ref button.Icon);
            Vector2 iconSize = Vector2.Zero;

            if (icon != null)
            {
                if (button.FitIcon)
                {
                    float widthLeftForIcon = innerBounds.Width - textWidth - button.IconTextSeparation;
                    float heightLeftForIcon = innerBounds.Height;

                    if (widthLeftForIcon <= 0 || heightLeftForIcon <= 0)
                    {
                        iconSize = Vector2.Zero;
                    }
                    else
                    {
                        iconSize = new Vector2(icon.Width, icon.Height);

                        float ratioLeftForIcon = heightLeftForIcon / widthLeftForIcon;
                        float iconRatio = iconSize.Y / iconSize.X;

                        if (ratioLeftForIcon > iconRatio)
                        {
                            iconSize = ResizeKeepAspect(iconSize, widthLeftForIcon, null);
                        }
                        else
                        {
                            iconSize = ResizeKeepAspect(iconSize, null, heightLeftForIcon);
                        }
                    }

                } else
                {
                    iconSize = GetButtonIconSize(ref button, icon);
                }
                totalWidth += iconSize.X;
                if (!string.IsNullOrEmpty(button.Text))
                {
                    totalWidth += button.IconTextSeparation;
                }
            }

            float x;
            switch (button.Alignment)
            {
                case Alignment.Begin:
                    x = innerBounds.X;
                    break;
                case Alignment.Center:
                    x = innerBounds.Center.X - totalWidth / 2;
                    break;
                case Alignment.End:
                    x = innerBounds.X + innerBounds.Width - totalWidth;
                    break;
                default:
                    x = innerBounds.X;
                    break;
            }

            if (icon != null)
            {
                layer.Surface.Draw(new DrawRectInstruction
                {
                    Texture = icon,
                    Color = button.IconColor,
                    DestinationRectangle = new Rectangle((int)x,
                        (int)(innerBounds.Center.Y - iconSize.Y / 2), (int) iconSize.X, (int) iconSize.Y).Translate(offset),
                    Z = -control.ZOrder
                });
                x += iconSize.X;
                x += button.IconTextSeparation;
            }

            renderedText.DrawOnto(layer.Surface,
                new Point((int)x, innerBounds.Center.Y - renderedText.Height / 2 + button.FontSize) + offset,
                button.Color, z: -control.ZOrder);
        }


        private void SubscribeToButtonEvents()
        {
            Subscribe((ref MouseEnteredEvent evt) =>
            {
                if (Scene.Components.HasComponent<ButtonControl>(evt.EntityId))
                {
                    Scene.Components.GetComponent<ButtonControl>(evt.EntityId).Rollover = true;
                }
            });
            Subscribe((ref MouseExitedEvent evt) =>
            {
                if (Scene.Components.HasComponent<ButtonControl>(evt.EntityId))
                {
                    Scene.Components.GetComponent<ButtonControl>(evt.EntityId).Rollover = false;
                }
            });
            Subscribe((ref MouseButtonEvent evt) =>
            {
                if (evt.EntityId != -1 && Scene.Components.HasComponent<ButtonControl>(evt.EntityId))
                {
                    ref var button = ref Scene.Components.GetComponent<ButtonControl>(evt.EntityId);
                    if (((int)button.ButtonMask & (1 << evt.Button)) == 0) return;

                    Console.WriteLine($"Button {evt.Type}");
                    var performAction = false;
                    switch (evt.Type)
                    {
                        case MouseEventType.Pressed:
                        {
                            button.Pressed = true;
                            evt.Consume();
                            if (button is { ActionMode: MouseActionMode.Press })
                            {
                                performAction = true;
                            }

                            break;
                        }
                        case MouseEventType.Released:
                        {
                            button.Pressed = false;
                            evt.Consume();
                            if (button is { ActionMode: MouseActionMode.Release, Rollover: true })
                            {
                                performAction = true;
                            }

                            break;
                        }
                    }

                    if (performAction)
                    {
                        Console.WriteLine($"Click!");
                        Scene.Events.Invoke(new ButtonPressedEvent
                        {
                            EntityId = evt.EntityId,
                            Position = evt.Position
                        });
                    }
                }
            });
            Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.EntityId != -1 && Scene.Components.HasComponent<ButtonControl>(evt.EntityId) &&
                    Scene.Components.HasComponent<Control>(evt.EntityId))
                {
                    ref var button = ref Scene.Components.GetComponent<ButtonControl>(evt.EntityId);
                    ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);

                    button.UpdateRenderedText(Scene.Core.RenderingUnit);
                    
                    control.ComputedMinimumSize = new Vector2(button.RenderedText.Width, button.RenderedText.Height);

                    var icon = Scene.Resources.Get<Texture>(ref button.Icon);

                    if (!button.FitIcon && icon != null)
                    {
                        var iconSize = GetButtonIconSize(ref button, icon);

                        control.ComputedMinimumSize.X += iconSize.X;
                        control.ComputedMinimumSize.Y = Math.Max(control.ComputedMinimumSize.Y, iconSize.Y);

                        if (!string.IsNullOrEmpty(button.Text))
                        {
                            control.ComputedMinimumSize.X += button.IconTextSeparation;
                        }
                    }
                }
            });
        }
        private static Vector2 GetButtonIconSize(ref ButtonControl button, ITexture icon)
        {
            var iconSize = new Vector2(icon.Width, icon.Height);
            if (button.IconMaxWidth > 0 && iconSize.X > button.IconMaxWidth)
            {
                iconSize = ResizeKeepAspect(iconSize, button.IconMaxWidth, null);
            }

            return iconSize;
        }
        private static Vector2 ResizeKeepAspect(Vector2 size, float? targetWidth, float? targetHeight)
        {
            if (targetWidth.HasValue)
            {
                size = new Vector2(targetWidth.Value, size.Y / size.X * targetWidth.Value);
            }
            
            if (targetHeight.HasValue)
            {
                size = new Vector2(size.X / size.Y * targetHeight.Value, targetHeight.Value);
            }

            return size;
        }
    }
}