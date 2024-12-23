using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public readonly struct GamePads
{
    public readonly GamePad[] Gamepads;

    public GamePads()
    {
        Gamepads = new GamePad[Microsoft.Xna.Framework.Input.GamePad.MaximumGamePadCount];
    }

    public GamePad this[int index] => Gamepads[index] ?? (Gamepads[index] = new GamePad(index));

    public void Update(InputUnit inputUnit)
    {
        if (!inputUnit.Core.FoldGame.IsActive) return;
        
        for (var index = 0; index < Gamepads.Length; index++)
        {
            var gamepad = Gamepads[index];
            gamepad?.Update(inputUnit);
        }
    }
}