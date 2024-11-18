using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.interface", ProcessingCycles.Input, true)]
public class ControlInterfaceSystem : GameSystem
{
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseLeft = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseMiddle = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseRight = ButtonAction.Default;
    
    private ComponentIterator<Viewport> _viewports;
    private ComponentIterator<Control> _controls;
    
    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
    }

    public override void OnInput()
    {
        var inputUnit = Scene.Core.InputUnit;
        
        if (MouseLeft == ButtonAction.Default)
        {
            MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
            MouseMiddle = new ButtonAction(inputUnit.Devices.Mouse.MiddleButton);
            MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
        }

        _viewports.Reset();
        while (_viewports.Next())
        {
            ref var viewport = ref _viewports.GetComponent();
            HandleViewport(ref viewport, inputUnit);
            break;
        }
    }

    private void HandleViewport(ref Viewport viewport, InputUnit inputUnit)
    {
        var layer = viewport.GetLayer(Scene.Core.RenderingUnit);
        if (layer == null) return;
        
        var mousePos = Mouse.GetState().Position;
        try
        {
            mousePos = layer.WindowToLayer(mousePos.ToVector2()).ToPoint();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        HandleMouseEvents(MouseLeft, MouseEvent.LeftButton);
        HandleMouseEvents(MouseMiddle, MouseEvent.MiddleButton);
        HandleMouseEvents(MouseRight, MouseEvent.RightButton);

        viewport.HoverTargetId = GetControlAtPoint(mousePos);
        Console.WriteLine($"Hover target: {viewport.HoverTargetId}");

        //
        // FocusOwner?.OnInput(ControlScheme);
    }

    private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex)
    {
        // if (mouseButton.Pressed)
        // {
        //     for (int i = VisiblePanels.Count - 1; i >= 0; i--)
        //     {
        //         GuiPanel panel = VisiblePanels[i];
        //         if (panel.Visible && panel.Bounds.Contains(MousePos))
        //         {
        //             _pressedPanels[buttonIndex] = panel;
        //
        //             var evt = new MouseEvent
        //             {
        //                 Type = MouseEventType.Pressed,
        //                 Position = MousePos,
        //                 Button = buttonIndex,
        //                 When = Time.Now
        //             };
        //
        //             panel.OnMousePressed(ref evt);
        //             break;
        //         }
        //     }
        // }
        // else if (mouseButton.Released)
        // {
        //     var evt = new MouseEvent
        //     {
        //         Type = MouseEventType.Released,
        //         Position = MousePos,
        //         Button = buttonIndex,
        //         When = Time.Now
        //     };
        //
        //     _pressedPanels[buttonIndex]?.OnMouseReleased(ref evt);
        //     _pressedPanels[buttonIndex] = null;
        // }
    }

    private long GetControlAtPoint(Point point)
    {
        _controls.Reset();

        float? topZ = 0;
        long topEntity = -1;
        
        while (_controls.Next())
        {
            ref var transform = ref _controls.GetCoComponent<Transform>();
            ref var control = ref _controls.GetComponent();

            if (topEntity == -1 || control.ZOrder >= topZ)
            {
                if (!Scene.Components.HasTrait<MousePickable>(_controls.GetEntityId())) continue;
                
                var position = transform.Position;
                var containsPoint = (position.X <= point.X && point.X <= position.X + control.Size.X) &&
                                    (position.Y <= point.Y && point.Y <= position.Y + control.Size.Y);
                
                if (containsPoint)
                {
                    topZ = control.ZOrder;
                    topEntity = _controls.GetEntityId();
                }
            }
        }

        return topEntity;
    }
}