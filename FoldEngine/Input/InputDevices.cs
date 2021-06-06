using System;
using System.Collections.Generic;

namespace FoldEngine.Input {
    public class InputDevices {
        public Keyboard Keyboard = new Keyboard();
        public Mouse Mouse = new Mouse();
        public GamePads GamePads = new GamePads();

        public IInputDevice this[string identifier] {
            get {
                switch(identifier) {
                    case "keyboard": return Keyboard;
                    case "mouse": return Mouse;
                    default: break;
                }

                if(identifier.StartsWith("gamepad:")) {
                    int gamepadIndex = int.Parse(identifier.Substring("gamepad:".Length));
                    return GamePads[gamepadIndex];
                }
                
                throw new ArgumentException($"Unknown input device identifier: {identifier}");
            }
        }

        public InputDevices() { }

        public void Update() {
            Keyboard.Update();
            Mouse.Update();
            GamePads.Update();
        }
    }
}