using System;

namespace FoldEngine.Input {
    public interface IAction {
    }

    public class ButtonAction : IAction {
        private ButtonInfo _buttonInfo;
        
        public bool Consumed => _buttonInfo.Pressed && ConsumeTime >= _buttonInfo.Since;
        public long ConsumeTime;
        
        public bool Pressed => _buttonInfo.Pressed;

        public int BufferTime = 16; // ms

        public ButtonAction(ButtonInfo buttonInfo) {
            _buttonInfo = buttonInfo;
        }

        public bool Consume() {
            if(_buttonInfo.Pressed && _buttonInfo.MillisecondsElapsed <= BufferTime && !Consumed) {
                ConsumeTime = Time.UnixNow;
                return true;
            }

            return false;
        }
    }

    public class AnalogAction : IAction {
        private Func<float> _provider;
        
        public AnalogAction(Func<float> provider) {
            this._provider = provider;
        }
        
        public static implicit operator float(AnalogAction action) {
            return action._provider();
        }
    }
}