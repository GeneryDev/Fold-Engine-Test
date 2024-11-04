namespace FoldEngine.Input;

public class GamePads
{
    public GamePad[] _gamepads = new GamePad[Microsoft.Xna.Framework.Input.GamePad.MaximumGamePadCount];

    public GamePad this[int index] => _gamepads[index] ?? (_gamepads[index] = new GamePad(index));

    public void Update()
    {
        foreach (GamePad gamepad in _gamepads) gamepad?.Update();
    }
}