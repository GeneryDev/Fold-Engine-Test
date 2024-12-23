using System;
using FoldEngine.Events;
using FoldEngine.ImmediateGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input;

[Event("fold:input", EventFlushMode.Immediate)]
public struct InputEvent
{
    public int DeviceIndex;

    public Type EventType;
    
    // Common Properties
    public bool Pressed;
    
    // Key Event Properties
    public Keys Key;
    public KeyModifiers Modifiers;
    public bool IsEcho;
    public char TypedCharacter;
    
    // Mouse Event Properties
    public MouseButtonMask MouseButtonMask;
    public Vector2 MousePosition;
    public MouseButtons MouseButton;
    public Point MouseWheelMovement;
    public Vector2 MouseRelative;

    // Gamepad Properties
    public Buttons Button;
    public GamePadAxis Axis;
    public float AxisValue;
    
    public bool Consumed;

    public bool Is<T>()
    {
        return typeof(T) == EventType;
    }

    public static explicit operator InputEventKey(InputEvent evt)
    {
        return new InputEventKey(evt);
    }

    public static explicit operator InputEventMouseButton(InputEvent evt)
    {
        return new InputEventMouseButton(evt);
    }

    public static explicit operator InputEventMouseWheelMotion(InputEvent evt)
    {
        return new InputEventMouseWheelMotion(evt);
    }

    public static explicit operator InputEventMouseMotion(InputEvent evt)
    {
        return new InputEventMouseMotion(evt);
    }

    public static explicit operator InputEventGamepadButton(InputEvent evt)
    {
        return new InputEventGamepadButton(evt);
    }

    public static explicit operator InputEventGamepadMotion(InputEvent evt)
    {
        return new InputEventGamepadMotion(evt);
    }

    public override string ToString()
    {
        return $"{EventType?.Name ?? nameof(InputEvent)}[{nameof(DeviceIndex)}: {DeviceIndex}, {EventTypeToString()}]";
    }

    private string EventTypeToString()
    {
        if (EventType == typeof(InputEventKey)) return ((InputEventKey)this).ToString();
        if (EventType == typeof(InputEventMouseButton)) return ((InputEventMouseButton)this).ToString();
        if (EventType == typeof(InputEventMouseWheelMotion)) return ((InputEventMouseWheelMotion)this).ToString();
        if (EventType == typeof(InputEventMouseMotion)) return ((InputEventMouseMotion)this).ToString();
        if (EventType == typeof(InputEventGamepadButton)) return ((InputEventGamepadButton)this).ToString();
        if (EventType == typeof(InputEventGamepadMotion)) return ((InputEventGamepadMotion)this).ToString();
        return "Invalid";
    }

    public void Consume()
    {
        Consumed = true;
    }
}

public interface IInputEventWrapper {}

public struct InputEventKey : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;
    
    public Keys Key
    {
        get => UnderlyingEvent.Key;
        set => UnderlyingEvent.Key = value;
    }

    public KeyModifiers Modifiers
    {
        get => UnderlyingEvent.Modifiers;
        set => UnderlyingEvent.Modifiers = value;
    }

    public bool Pressed
    {
        get => UnderlyingEvent.Pressed;
        set => UnderlyingEvent.Pressed = value;
    }

    public bool IsEcho
    {
        get => UnderlyingEvent.IsEcho;
        set => UnderlyingEvent.IsEcho = value;
    }

    public char Character
    {
        get => UnderlyingEvent.TypedCharacter;
        set => UnderlyingEvent.TypedCharacter = value;
    }

    public InputEventKey(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventKey() : this(0)
    {
    }

    public InputEventKey(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(Key)}: {Key}, {nameof(Modifiers)}: {Modifiers}, {nameof(Pressed)}: {Pressed}, {nameof(IsEcho)}: {IsEcho}, {nameof(Character)}: {Character}";
    }
}

public struct InputEventMouseButton : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;

    public MouseButtonMask ButtonMask
    {
        get => UnderlyingEvent.MouseButtonMask;
        set => UnderlyingEvent.MouseButtonMask = value;
    }

    public Vector2 Position
    {
        get => UnderlyingEvent.MousePosition;
        set => UnderlyingEvent.MousePosition = value;
    }

    public KeyModifiers Modifiers
    {
        get => UnderlyingEvent.Modifiers;
        set => UnderlyingEvent.Modifiers = value;
    }

    public MouseButtons Button
    {
        get => UnderlyingEvent.MouseButton;
        set => UnderlyingEvent.MouseButton = value;
    }

    public bool Pressed
    {
        get => UnderlyingEvent.Pressed;
        set => UnderlyingEvent.Pressed = value;
    }

    public InputEventMouseButton(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventMouseButton() : this(0)
    {
    }

    public InputEventMouseButton(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(ButtonMask)}: {ButtonMask}, {nameof(Position)}: {Position}, {nameof(Modifiers)}: {Modifiers}, {nameof(Button)}: {Button}, {nameof(Pressed)}: {Pressed}";
    }
}

public struct InputEventMouseWheelMotion : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;

    public MouseButtonMask ButtonMask
    {
        get => UnderlyingEvent.MouseButtonMask;
        set => UnderlyingEvent.MouseButtonMask = value;
    }

    public Vector2 Position
    {
        get => UnderlyingEvent.MousePosition;
        set => UnderlyingEvent.MousePosition = value;
    }

    public KeyModifiers Modifiers
    {
        get => UnderlyingEvent.Modifiers;
        set => UnderlyingEvent.Modifiers = value;
    }

    public int Amount
    {
        get => UnderlyingEvent.MouseWheelMovement.Y;
        set => UnderlyingEvent.MouseWheelMovement.Y = value;
    }

    public int HorizontalAmount
    {
        get => UnderlyingEvent.MouseWheelMovement.X;
        set => UnderlyingEvent.MouseWheelMovement.X = value;
    }

    public InputEventMouseWheelMotion(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventMouseWheelMotion() : this(0)
    {
    }

    public InputEventMouseWheelMotion(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(ButtonMask)}: {ButtonMask}, {nameof(Position)}: {Position}, {nameof(Modifiers)}: {Modifiers}, {nameof(Amount)}: {Amount}, {nameof(HorizontalAmount)}: {HorizontalAmount}";
    }
}

public struct InputEventMouseMotion : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;

    public MouseButtonMask ButtonMask
    {
        get => UnderlyingEvent.MouseButtonMask;
        set => UnderlyingEvent.MouseButtonMask = value;
    }

    public Vector2 Position
    {
        get => UnderlyingEvent.MousePosition;
        set => UnderlyingEvent.MousePosition = value;
    }

    public KeyModifiers Modifiers
    {
        get => UnderlyingEvent.Modifiers;
        set => UnderlyingEvent.Modifiers = value;
    }

    public Vector2 Relative
    {
        get => UnderlyingEvent.MouseRelative;
        set => UnderlyingEvent.MouseRelative = value;
    }

    public InputEventMouseMotion(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventMouseMotion() : this(0)
    {
    }

    public InputEventMouseMotion(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(ButtonMask)}: {ButtonMask}, {nameof(Position)}: {Position}, {nameof(Modifiers)}: {Modifiers}, {nameof(Relative)}: {Relative}";
    }
}

public struct InputEventGamepadButton : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;

    public Buttons Button
    {
        get => UnderlyingEvent.Button;
        set => UnderlyingEvent.Button = value;
    }

    public bool Pressed
    {
        get => UnderlyingEvent.Pressed;
        set => UnderlyingEvent.Pressed = value;
    }

    public InputEventGamepadButton(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventGamepadButton() : this(0)
    {
    }

    public InputEventGamepadButton(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(Button)}: {Button}, {nameof(Pressed)}: {Pressed}";
    }
}

public struct InputEventGamepadMotion : IInputEventWrapper
{
    public InputEvent UnderlyingEvent;

    public GamePadAxis Axis
    {
        get => UnderlyingEvent.Axis;
        set => UnderlyingEvent.Axis = value;
    }

    public float AxisValue
    {
        get => UnderlyingEvent.AxisValue;
        set => UnderlyingEvent.AxisValue = value;
    }

    public InputEventGamepadMotion(InputEvent underlyingEvent)
    {
        this.UnderlyingEvent = underlyingEvent;
    }

    public InputEventGamepadMotion() : this(0)
    {
    }

    public InputEventGamepadMotion(int deviceIndex = 0)
    {
        UnderlyingEvent = new InputEvent()
        {
            DeviceIndex = deviceIndex,
            EventType = this.GetType()
        };
    }

    public override string ToString()
    {
        return $"{nameof(Axis)}: {Axis}, {nameof(AxisValue)}: {AxisValue}";
    }
}

public enum GamePadAxis
{
    None,
    LeftX,
    LeftY,
    RightX,
    RightY,
    TriggerLeft,
    TriggerRight
}