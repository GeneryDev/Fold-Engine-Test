using System;

namespace FoldEngine.Input {
    public interface IInputInfo { }

    public class ButtonInfo : IInputInfo {
        public bool Pressed;
        public long Since;
        public Func<bool> Lookup;

        public long MillisecondsElapsed => Since - Time.UnixNow;

        public ButtonInfo(Func<bool> lookup) {
            Lookup = lookup;
            Update();
        }

        public void Update() {
            bool nowPressed = Lookup();
            if(nowPressed != Pressed) {
                Since = Time.UnixNow;
                Pressed = nowPressed;
            }
        }
    }
}