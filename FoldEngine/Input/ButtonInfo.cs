﻿using System;

namespace FoldEngine.Input {
    public interface IInputInfo {
        void Update();
    }

    public class ButtonInfo : IInputInfo {
        public bool Down;
        public long Since;
        public long SinceFrame;
        public Func<bool> Lookup;

        public long MillisecondsElapsed => Time.Now - Since;

        public ButtonInfo(Func<bool> lookup) {
            Lookup = lookup;
            Update();
        }

        public void Update() {
            bool nowDown = Lookup();
            if(nowDown != Down) {
                Since = Time.Now;
                SinceFrame = Time.Frame;
                Down = nowDown;
            }
        }
    }
}