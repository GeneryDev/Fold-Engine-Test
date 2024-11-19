using System;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using MouseEventType = FoldEngine.Gui.Events.MouseEventType;

namespace FoldEngine.Gui.Systems;

public partial class ControlRenderer
{
    private void RenderButton(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform, ref Control control, ref ButtonControl button)
    {
        var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
        Point offset = Point.Zero;
        
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = button.Pressed && (button.KeepPressedOutside || button.Rollover)
                ? button.PressedColor
                : button.Rollover
                    ? button.RolloverColor
                    : button.NormalColor,
            DestinationRectangle = bounds.Translate(offset),
            Z = -control.ZOrder
        });

        if (button.UpdateRenderedText(renderer))
        {
            control.ComputedMinimumSize = new Vector2(button.RenderedText.Width, button.RenderedText.Height);
            control.RequestLayout = true;
        }
        ref RenderedText renderedText = ref button.RenderedText;
        if (!renderedText.HasValue) return;

        float textWidth = renderedText.Width;

        int totalWidth = (int)textWidth;

        int x;
        switch (button.Alignment)
        {
            case Alignment.Begin:
                x = bounds.X;
                break;
            case Alignment.Center:
                x = bounds.Center.X - totalWidth / 2;
                break;
            case Alignment.End:
                x = bounds.X + bounds.Width - totalWidth;
                break;
            default:
                x = bounds.X;
                break;
        }

        renderedText.DrawOnto(layer.Surface, new Point(x, bounds.Center.Y - renderedText.Height / 2 + button.FontSize) + offset,
            button.Color, z: -control.ZOrder);
        // layer.Surface.Draw(new DrawRectInstruction
        // {
        //     Texture = renderer.WhiteTexture,
        //     Color = Color.White,
        //     DestinationRectangle = new Rectangle(new Point(x - 2, bounds.Center.Y - renderedText.Height / 2 + label.FontSize), new Point(2, 2)),
        //     Z = control.ZOrder
        // });
    }

    public override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
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
    }
}