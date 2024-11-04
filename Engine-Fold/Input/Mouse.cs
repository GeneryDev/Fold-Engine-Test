using System;
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

    public void Update()
    {
        LeftButton.Update();
        MiddleButton.Update();
        RightButton.Update();
        ScrollWheel.Update();
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