using System;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input;

public class GamePad : IInputDevice
{
    public GamePadButtons Buttons;
    public GamePadDPad DPad;
    public GamePadThumbsticks Thumbsticks;
    public GamePadTriggers Triggers;

    public GamePad(int playerIndex)
    {
        Buttons = new GamePadButtons(playerIndex);
        Thumbsticks = new GamePadThumbsticks(playerIndex);
        DPad = new GamePadDPad(playerIndex);
        Triggers = new GamePadTriggers(playerIndex);
    }

    public bool IsBeingUsed =>
        Thumbsticks.IsBeingUsed || Buttons.IsBeingUsed || Triggers.IsBeingUsed || DPad.IsBeingUsed;

    public void Update()
    {
        Buttons.Update();
        DPad.Update();
        Thumbsticks.Update();
        Triggers.Update();
    }


    public T Get<T>(string name) where T : IInputInfo
    {
        if (typeof(T) == typeof(ButtonInfo))
            switch (name.ToLowerInvariant())
            {
                case "a": return (T)(IInputInfo)Buttons.A;
                case "b": return (T)(IInputInfo)Buttons.B;
                case "x": return (T)(IInputInfo)Buttons.X;
                case "y": return (T)(IInputInfo)Buttons.Y;
                case "start": return (T)(IInputInfo)Buttons.Start;
                case "back": return (T)(IInputInfo)Buttons.Back;
                case "home": return (T)(IInputInfo)Buttons.Home;
                case "leftbumper": return (T)(IInputInfo)Buttons.LeftBumper;
                case "rightbumper": return (T)(IInputInfo)Buttons.RightBumper;
                case "lefttrigger": return (T)(IInputInfo)Buttons.LeftTrigger;
                case "righttrigger": return (T)(IInputInfo)Buttons.RightTrigger;
                case "leftstick": return (T)(IInputInfo)Buttons.LeftStick;
                case "rightstick": return (T)(IInputInfo)Buttons.RightStick;

                case "dpad.up": return (T)(IInputInfo)DPad.Up;
                case "dpad.down": return (T)(IInputInfo)DPad.Down;
                case "dpad.left": return (T)(IInputInfo)DPad.Left;
                case "dpad.right": return (T)(IInputInfo)DPad.Right;
            }
        else if (typeof(T) == typeof(IAnalogInfo) || typeof(T).IsSubclassOf(typeof(IAnalogInfo)))
            switch (name.ToLowerInvariant())
            {
                case "thumbstick.left": return (T)(IInputInfo)Thumbsticks.Left;
                case "thumbstick.right": return (T)(IInputInfo)Thumbsticks.Right;
            }

        throw new ArgumentException(name);
    }


    public class GamePadButtons
    {
        private const double TriggerThreshold = 0.2;

        public ButtonInfo A;
        public ButtonInfo B;
        public ButtonInfo Back;
        public ButtonInfo Home;
        public ButtonInfo LeftBumper;
        public ButtonInfo LeftStick;

        public ButtonInfo LeftTrigger;
        public ButtonInfo RightBumper;
        public ButtonInfo RightStick;
        public ButtonInfo RightTrigger;
        public ButtonInfo Start;
        public ButtonInfo X;
        public ButtonInfo Y;

        public GamePadButtons(int playerId)
        {
            A = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.A == ButtonState.Pressed);
            B = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.B == ButtonState.Pressed);
            X = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.X == ButtonState.Pressed);
            Y = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.Y == ButtonState.Pressed);
            Start = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.Start == ButtonState.Pressed);
            Back = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.Back == ButtonState.Pressed);
            Home = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.BigButton == ButtonState.Pressed);
            LeftBumper = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.LeftShoulder
                == ButtonState.Pressed);
            RightBumper = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.RightShoulder
                == ButtonState.Pressed);
            LeftStick = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.LeftStick == ButtonState.Pressed);
            RightStick = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Buttons.RightStick == ButtonState.Pressed);
            LeftTrigger = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Triggers.Left >= TriggerThreshold);
            RightTrigger = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).Triggers.Right >= TriggerThreshold);
        }

        public bool IsBeingUsed => A.Down
                                   || B.Down
                                   || X.Down
                                   || Y.Down
                                   || Start.Down
                                   || Back.Down
                                   || Home.Down
                                   || LeftBumper.Down
                                   || RightBumper.Down
                                   || LeftStick.Down
                                   || RightStick.Down;


        public void Update()
        {
            A.Update();
            B.Update();
            X.Update();
            Y.Update();
            Start.Update();
            Back.Update();
            Home.Update();
            LeftBumper.Update();
            RightBumper.Update();
            LeftStick.Update();
            RightStick.Update();
            LeftTrigger.Update();
            RightTrigger.Update();
        }
    }

    public class GamePadTriggers
    {
        public AnalogInfo1 Left;
        public AnalogInfo1 Right;

        public GamePadTriggers(int playerIndex)
        {
            Left = new AnalogInfo1(() => Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex).Triggers.Left);
            Right = new AnalogInfo1(
                () => Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex).Triggers.Right);
        }

        public bool IsBeingUsed => Left.MagnitudeSqr > 0 || Right.MagnitudeSqr > 0;

        public void Update()
        {
            Left.Update();
            Right.Update();
        }
    }

    public class GamePadDPad
    {
        public ButtonInfo Down;
        public ButtonInfo Left;
        public ButtonInfo Right;
        public ButtonInfo Up;

        public GamePadDPad(int playerId)
        {
            Up = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).DPad.Up == ButtonState.Pressed);
            Down = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).DPad.Down == ButtonState.Pressed);
            Left = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).DPad.Left == ButtonState.Pressed);
            Right = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerId).DPad.Right == ButtonState.Pressed);
        }

        public bool IsBeingUsed => Up.Down || Down.Down || Left.Down || Right.Down;

        public void Update()
        {
            Up.Update();
            Down.Update();
            Left.Update();
            Right.Update();
        }
    }

    public class GamePadThumbsticks
    {
        public AnalogInfo2 Left;
        public AnalogInfo2 Right;

        public GamePadThumbsticks(int playerIndex)
        {
            Left = new AnalogInfo2(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex).ThumbSticks.Left);
            Right = new AnalogInfo2(() =>
                Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex).ThumbSticks.Right);
        }

        public bool IsBeingUsed => Left.MagnitudeSqr > 0 || Right.MagnitudeSqr > 0;

        public void Update()
        {
            Left.Update();
            Right.Update();
        }
    }
}