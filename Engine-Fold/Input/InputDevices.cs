using System;
using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public class InputDevices
{
    public InputUnit InputUnit;
    
    public GamePads GamePads = new GamePads();
    public Keyboard Keyboard = new Keyboard();
    public Mouse Mouse = new Mouse();

    public InputDevices(InputUnit inputUnit)
    {
        InputUnit = inputUnit;
        Mouse.Keyboard = Keyboard;
    }

    public IInputDevice this[string identifier]
    {
        get
        {
            switch (identifier)
            {
                case "keyboard": return Keyboard;
                case "mouse": return Mouse;
            }

            if (identifier.StartsWith("gamepad:"))
            {
                int gamepadIndex = int.Parse(identifier.Substring("gamepad:".Length));
                return GamePads[gamepadIndex];
            }

            throw new ArgumentException($"Unknown input device identifier: {identifier}");
        }
    }

    public void Update()
    {
        Keyboard.Update(InputUnit);
        Mouse.Update(InputUnit);
        GamePads.Update(InputUnit);
    }
}