using System;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input;

public class Mouse : IInputDevice
{
    public readonly ButtonInfo LeftButton = new ButtonInfo(() =>
        Microsoft.Xna.Framework.Input.Mouse.GetState().LeftButton == ButtonState.Pressed);

    public readonly ButtonInfo MiddleButton = new ButtonInfo(() =>
        Microsoft.Xna.Framework.Input.Mouse.GetState().MiddleButton == ButtonState.Pressed);

    public readonly ButtonInfo RightButton = new ButtonInfo(() =>
        Microsoft.Xna.Framework.Input.Mouse.GetState().RightButton == ButtonState.Pressed);

    public readonly AnalogInfo1 ScrollWheel =
        new AnalogInfo1(() => Microsoft.Xna.Framework.Input.Mouse.GetState().ScrollWheelValue);

    public bool IsBeingUsed => LeftButton.Down || MiddleButton.Down || RightButton.Down;
    public Keyboard Keyboard;

    private MouseState _prevState;

    public void Update(InputUnit inputUnit)
    {
        if (!inputUnit.Core.FoldGame.IsActive) return;
        
        LeftButton.Update();
        MiddleButton.Update();
        RightButton.Update();
        ScrollWheel.Update();
        
        var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

        HandleMouseMotion(mouseState.Position, _prevState.Position,
            inputUnit, mouseState);
        HandleMouseButton(MouseButtons.LeftButton,
            mouseState.LeftButton == ButtonState.Pressed,
            _prevState.LeftButton == ButtonState.Pressed,
            inputUnit, mouseState);
        HandleMouseButton(MouseButtons.MiddleButton,
            mouseState.MiddleButton == ButtonState.Pressed,
            _prevState.MiddleButton == ButtonState.Pressed,
            inputUnit, mouseState);
        HandleMouseButton(MouseButtons.RightButton,
            mouseState.RightButton == ButtonState.Pressed,
            _prevState.RightButton == ButtonState.Pressed,
            inputUnit, mouseState);
        HandleMouseButton(MouseButtons.XButton1,
            mouseState.XButton1 == ButtonState.Pressed,
            _prevState.XButton1 == ButtonState.Pressed,
            inputUnit, mouseState);
        HandleMouseButton(MouseButtons.XButton2,
            mouseState.XButton2 == ButtonState.Pressed,
            _prevState.XButton2 == ButtonState.Pressed,
            inputUnit, mouseState);
        HandleMouseWheel(mouseState.ScrollWheelValue, _prevState.ScrollWheelValue, false,
            inputUnit, mouseState);
        HandleMouseWheel(mouseState.HorizontalScrollWheelValue, _prevState.HorizontalScrollWheelValue, true,
            inputUnit, mouseState);

        _prevState = mouseState;
    }

    private void HandleMouseMotion(Point currentPos, Point prevPos, InputUnit inputUnit, MouseState state)
    {
        if (currentPos != prevPos)
        {
            inputUnit.InvokeInputEvent(new InputEventMouseMotion(-1)
            {
                Relative = (currentPos - prevPos).ToVector2(),
                ButtonMask = GetButtonMask(state),
                Position = state.Position.ToVector2(),
                Modifiers = Keyboard?.GetKeyModifiers() ?? KeyModifiers.None,
            }.UnderlyingEvent);
        }
    }

    private void HandleMouseButton(MouseButtons button, bool isDown, bool wasDown, InputUnit inputUnit, MouseState state)
    {
        if (isDown != wasDown)
        {
            inputUnit.InvokeInputEvent(new InputEventMouseButton(-1)
            {
                Button = button,
                ButtonMask = GetButtonMask(state),
                Position = state.Position.ToVector2(),
                Modifiers = Keyboard?.GetKeyModifiers() ?? KeyModifiers.None,
                Pressed = isDown,
            }.UnderlyingEvent);
        }
    }

    private void HandleMouseWheel(int amount, int prevAmount, bool horizontal, InputUnit inputUnit, MouseState state)
    {
        if (amount != prevAmount)
        {
            inputUnit.InvokeInputEvent(new InputEventMouseWheelMotion(-1)
            {
                Amount = horizontal ? 0 : amount - prevAmount,
                HorizontalAmount = horizontal ? amount - prevAmount : 0,
                ButtonMask = GetButtonMask(state),
                Position = state.Position.ToVector2(),
                Modifiers = Keyboard?.GetKeyModifiers() ?? KeyModifiers.None,
            }.UnderlyingEvent);
        }
    }

    private MouseButtonMask GetButtonMask(MouseState state)
    {
        MouseButtonMask mask = 0;
        if (state.LeftButton == ButtonState.Pressed) mask |= MouseButtonMask.LeftButton;
        if (state.MiddleButton == ButtonState.Pressed) mask |= MouseButtonMask.MiddleButton;
        if (state.RightButton == ButtonState.Pressed) mask |= MouseButtonMask.RightButton;
        if (state.XButton1 == ButtonState.Pressed) mask |= MouseButtonMask.XButton1;
        if (state.XButton2 == ButtonState.Pressed) mask |= MouseButtonMask.XButton2;
        return mask;
    }

    public MouseButtonMask GetButtonMask() => GetButtonMask(_prevState);

    public bool IsButtonDown(MouseButtons button)
    {
        return GetButtonMask().Has(button);
    }

    public T Get<T>(string name) where T : IInputInfo
    {
        if (typeof(T) == typeof(ButtonInfo))
            switch (name.ToLowerInvariant())
            {
                case "left": return (T)(IInputInfo)LeftButton;
                case "middle": return (T)(IInputInfo)MiddleButton;
                case "right": return (T)(IInputInfo)RightButton;
            }
        else if (typeof(T) == typeof(IAnalogInfo))
            switch (name.ToLowerInvariant())
            {
                case "wheel": return (T)(IInputInfo)ScrollWheel;
            }

        throw new ArgumentException(name);
    }
}