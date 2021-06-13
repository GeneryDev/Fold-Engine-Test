using System;
using Microsoft.Xna.Framework;

namespace FoldEngine.Input {
    public interface IAnalogInfo : IInputInfo {
        float this[int axis] { get; }
        int Grade { get; }

        float MagnitudeSqr { get; }
        float Magnitude { get; }

        long LastChangedTime { get; }
        float GetChange(int axis);
    }

    public class AnalogInfo1 : IAnalogInfo {
        private float _value = 0;
        private long _lastChangedTime = Time.Now;
        private float _change;
        private Func<float> _getter;

        public float this[int axis] {
            get {
                if(axis == 0) return Value;
                throw new ArgumentException(nameof(axis));
            }
        }

        private float Value => _value;
        public long LastChangedTime => _lastChangedTime;
        public float Change => _change;

        public int Grade => 1;
        
        public float MagnitudeSqr => Value;
        public float Magnitude => (float) Math.Sqrt(MagnitudeSqr);

        public AnalogInfo1(Func<float> getter) {
            this._getter = getter;
        }

        public static implicit operator float(AnalogInfo1 info) {
            return info.Value;
        }

        public void Update() {
            float oldValue = _value;
            _value = _getter();
            if(_value != oldValue) {
                _lastChangedTime = Time.Now;
                _change = _value - oldValue;
            }
        }

        public float GetChange(int axis) {
            return Change;
        }
    }

    public class AnalogInfo2 : IAnalogInfo {
        private Func<Vector2> _getter;
        private Vector2 _value = Vector2.Zero;
        private long _lastChangedTime = Time.Now;
        private Vector2 _change;

        private Vector2 Value => _value;
        public long LastChangedTime => _lastChangedTime;
        public Vector2 Change => _change;

        public float this[int axis] {
            get {
                switch(axis) {
                    case 0:
                        return Value.X;
                    case 1:
                        return Value.Y;
                    default:
                        throw new ArgumentException(nameof(axis));
                }
            }
        }

        public int Grade => 2;
        public float MagnitudeSqr => Value.LengthSquared();
        public float Magnitude => Value.Length();

        public AnalogInfo2(Func<Vector2> getter) {
            this._getter = getter;
        }

        public void Update() {
            Vector2 oldValue = _value;
            _value = _getter();
            if(_value != oldValue) {
                _lastChangedTime = Time.Now;
                _change = _value - oldValue;
            }
        }

        public float GetChange(int axis) {
            switch(axis) {
                case 0:
                    return Change.X;
                case 1:
                    return Change.Y;
                default:
                    throw new ArgumentException(nameof(axis));
            }
        }

        public static implicit operator Vector2(AnalogInfo2 info) {
            return info.Value;
        }
    }
}