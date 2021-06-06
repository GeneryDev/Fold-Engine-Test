using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input {
    public class Keyboard : IInputDevice {
        
        private List<ButtonInfo> _trackedKeyInfo = new List<ButtonInfo>();
        private List<Keys> _trackedKeys = new List<Keys>();

        public ButtonInfo this[Keys key] {
            get {
                int existingIndex = _trackedKeys.IndexOf(key);
                if(existingIndex != -1) {
                    return _trackedKeyInfo[existingIndex];
                } else {
                    var newInfo = new ButtonInfo(() =>
                        Microsoft.Xna.Framework.Input.Keyboard.GetState()[key] == KeyState.Down);
                    _trackedKeyInfo.Add(newInfo);
                    _trackedKeys.Add(key);
                    return newInfo;
                }
            }
        }

        public bool IsBeingUsed => Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys().Length > 0;
        public void Update() {
            foreach(ButtonInfo info in _trackedKeyInfo) {
                info.Update();
            }
        }
        
        public T Get<T>(string name) where T : IInputInfo {
            if(typeof(T) == typeof(ButtonInfo)) {
                Enum.TryParse(name, true, out Keys k);
                if(k != Keys.None) {
                    return (T)(IInputInfo)this[k];
                }
            }
            throw new ArgumentException(name);
        }
    }
}