using System;
using Microsoft.Xna.Framework;

namespace FoldEngine.Input {
    public interface IAnalogInfo : IInputInfo {
        float this[int axis] { get; }
        int Grade { get; }
        
        float MagnitudeSqr { get; }
        float Magnitude { get; }
    }

    public class AnalogInfo1 : IAnalogInfo {
        private float _value = 0;
        private bool _upToDate = false;
        private Func<float> _getter;

        public float this[int axis] {
            get {
                if(axis == 0) return Value;
                throw new ArgumentException(nameof(axis));
            }
        }

        private float Value {
            get {
                if(!_upToDate) {
                    _value = _getter();
                    _upToDate = true;
                }
                return _value;
            }
        }

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
            _upToDate = false;
        }
    }

    public class AnalogInfo2 : IAnalogInfo {
        private Func<Vector2> _getter;
        private Vector2 _value = Vector2.Zero;
        private bool _upToDate = false;

        private Vector2 Value {
            get {
                if(!_upToDate) {
                    _value = _getter();
                    _upToDate = true;
                }
                return _value;
            }
        }

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

        public static implicit operator Vector2(AnalogInfo2 info) {
            return info.Value;
        }

        public void Update() {
            _upToDate = false;
        }
    }
}