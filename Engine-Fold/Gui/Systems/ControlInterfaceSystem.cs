using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.interface", ProcessingCycles.Input, true)]
public class ControlInterfaceSystem : GameSystem
{
    private static readonly Point NullMousePos = new Point(-1, -1);

    [DoNotSerialize] [HideInInspector] public ButtonAction MouseLeft = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseMiddle = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseRight = ButtonAction.Default;
    
    private ComponentIterator<Viewport> _viewports;
    private ComponentIterator<Control> _controls;

    [DoNotSerialize] private Point _prevMousePos = NullMousePos;
    [DoNotSerialize] private Point _mousePos = NullMousePos;
    [DoNotSerialize] private readonly long[] _pressedControls = new long[MouseButtonEvent.MaxButtons];
    
    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
        
        Array.Fill(_pressedControls, -1L);
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
            HandleViewport(ref viewport);
            break;
        }
    }

    private void HandleViewport(ref Viewport viewport)
    {
        var layer = viewport.GetLayer(Scene.Core.RenderingUnit);
        if (layer == null) return;

        _prevMousePos = _mousePos;
        _mousePos = Mouse.GetState().Position;
        try
        {
            _mousePos = layer.WindowToLayer(_mousePos.ToVector2()).ToPoint();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        if (_prevMousePos == NullMousePos)
        {
            _prevMousePos = _mousePos;
        }

        viewport.PrevHoverTargetId = viewport.HoverTargetId;
        viewport.HoverTargetId = GetControlAtPoint(_mousePos);
        
        HandleMouseMotionEvents(ref viewport);

        if (viewport.PrevHoverTargetId != viewport.HoverTargetId)
        {
            if (viewport.PrevHoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseExitedEvent(viewport.PrevHoverTargetId, _mousePos));
            }
            if (viewport.HoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseEnteredEvent(viewport.HoverTargetId, _mousePos));
            }
        }
        
        HandleMouseEvents(MouseLeft, MouseButtonEvent.LeftButton);
        HandleMouseEvents(MouseMiddle, MouseButtonEvent.MiddleButton);
        HandleMouseEvents(MouseRight, MouseButtonEvent.RightButton);
        
        // Console.WriteLine($"Hover target: {viewport.HoverTargetId}");

        //
        // FocusOwner?.OnInput(ControlScheme);
    }

    private void HandleMouseMotionEvents(ref Viewport viewport)
    {
        var mouseDelta = _mousePos - _prevMousePos;

        if (mouseDelta == Point.Zero) return;
        Scene.Events.Invoke(new MouseMovedEvent
        {
            EntityId = viewport.HoverTargetId,
            Position = _mousePos,
            Delta = mouseDelta
        });
        for (var btnIndex = 0; btnIndex < MouseButtonEvent.MaxButtons; btnIndex++)
        {
            if (_pressedControls[btnIndex] == -1) continue;
            
            Scene.Events.Invoke(new MouseDraggedEvent()
            {
                EntityId = _pressedControls[btnIndex],
                Position = _mousePos,
                Delta = mouseDelta,
                Button = btnIndex
            });
        }
    }

    private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex)
    {
        if (mouseButton.Pressed)
        {
            long onEntityId = GetControlAtPoint(_mousePos);
            _pressedControls[buttonIndex] = onEntityId;
            Scene.Events.Invoke(new MouseButtonEvent
            {
                Type = MouseButtonEventType.Pressed,
                EntityId = onEntityId,
                Position = _mousePos,
                Button = buttonIndex
            });
        }
        else if (mouseButton.Released)
        {
            Scene.Events.Invoke(new MouseButtonEvent
            {
                Type = MouseButtonEventType.Released,
                EntityId = _pressedControls[buttonIndex],
                Position = _mousePos,
                Button = buttonIndex
            });
            _pressedControls[buttonIndex] = -1;
        }
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