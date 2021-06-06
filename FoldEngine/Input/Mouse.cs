using System;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input {
    public class Mouse : IInputDevice {

        public readonly ButtonInfo LeftButton = new ButtonInfo(() =>
            Microsoft.Xna.Framework.Input.Mouse.GetState().LeftButton == ButtonState.Pressed);
        
        public readonly ButtonInfo MiddleButton = new ButtonInfo(() =>
            Microsoft.Xna.Framework.Input.Mouse.GetState().MiddleButton == ButtonState.Pressed);
        
        public readonly ButtonInfo RightButton = new ButtonInfo(() =>
            Microsoft.Xna.Framework.Input.Mouse.GetState().RightButton == ButtonState.Pressed);

        public bool IsBeingUsed => LeftButton.Pressed || MiddleButton.Pressed || RightButton.Pressed;

        public void Update() {
            LeftButton.Update();
            MiddleButton.Update();
            RightButton.Update();
        }

        public T Get<T>(string name) where T : IInputInfo {
            if(typeof(T) == typeof(ButtonInfo)) {
                switch(name.ToLowerInvariant()) {
                    case "left": return (T)(IInputInfo)LeftButton;
                    case "middle": return (T)(IInputInfo)MiddleButton;
                    case "right": return (T)(IInputInfo)RightButton;
                }
            }
            throw new ArgumentException(name);
        }
    }
}