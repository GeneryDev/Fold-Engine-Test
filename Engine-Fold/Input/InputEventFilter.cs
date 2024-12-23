using System;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input;

public readonly struct InputEventFilter
{
    public readonly int DeviceIndex = -1;
    public readonly Type EventType;
    
    // Common Properties
    public readonly bool? Pressed;
    
    // Key Event Properties
    public readonly Keys? Key;
    public readonly KeyModifiers? Modifiers;
    public readonly bool? IsEcho;
    
    // Mouse Event Properties
    public readonly MouseButtons? MouseButton;
    public readonly Point? MouseWheelMovement;

    // Gamepad Properties
    public readonly Buttons? Button;
    public readonly GamePadAxis? Axis;
    public readonly float? AxisValue;

    public InputEventFilter()
    {
    }

    public bool Match(InputEvent evt)
    {
        if (DeviceIndex != -1 && evt.DeviceIndex != DeviceIndex) return false;
        if (EventType != null && evt.EventType != EventType) return false;
        if (Pressed != null && evt.Pressed != Pressed) return false;
        if (Key != null && evt.Key != Key) return false;
        if (Modifiers != null && (evt.Modifiers & Modifiers.Value) != Modifiers.Value) return false;
        if (IsEcho != null && evt.IsEcho != IsEcho) return false;
        if (MouseButton != null && evt.MouseButton != MouseButton) return false;
        if (MouseWheelMovement != null && Math.Sign(evt.MouseWheelMovement.X) != Math.Sign(MouseWheelMovement.Value.X) && Math.Sign(evt.MouseWheelMovement.Y) != Math.Sign(MouseWheelMovement.Value.Y)) return false;
        if (Button != null && evt.Button != Button) return false;
        if (Axis != null && evt.Axis != Axis) return false;
        if (AxisValue != null && Math.Sign(evt.AxisValue) != Math.Sign(AxisValue.Value)) return false;
        return true;
    }

    public bool IsDown(InputUnit inputUnit)
    {
        if (Pressed == false) return false;
        
        if (Key != null && inputUnit.Devices.Keyboard.IsKeyDown(Key.Value)) return true;
        if (MouseButton != null && inputUnit.Devices.Mouse.IsButtonDown(MouseButton.Value)) return true;
        if (Button != null && inputUnit.Devices.GamePads[Math.Max(0, DeviceIndex)].IsButtonDown(Button.Value)) return true;
        
        return false;
    }
}