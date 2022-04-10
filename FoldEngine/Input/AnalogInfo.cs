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
        private readonly Func<float> _getter;

        public AnalogInfo1(Func<float> getter) {
            _getter = getter;
        }

        private float Value { get; set; }

        public float Change { get; private set; }

        public float this[int axis] {
            get {
                if(axis == 0) return Value;
                throw new ArgumentException(nameof(axis));
            }
        }

        public long LastChangedTime { get; private set; } = Time.Now;

        public int Grade => 1;

        public float MagnitudeSqr => Value;
        public float Magnitude => (float) Math.Sqrt(MagnitudeSqr);

        public void Update() {
            float oldValue = Value;
            Value = _getter();
            if(Value != oldValue) {
                LastChangedTime = Time.Now;
                Change = Value - oldValue;
            }
        }

        public float GetChange(int axis) {
            return Change;
        }

        public static implicit operator float(AnalogInfo1 info) {
            return info.Value;
        }
    }

    public class AnalogInfo2 : IAnalogInfo {
        private readonly Func<Vector2> _getter;

        public AnalogInfo2(Func<Vector2> getter) {
            _getter = getter;
        }

        private Vector2 Value { get; set; } = Vector2.Zero;

        public Vector2 Change { get; private set; }

        public long LastChangedTime { get; private set; } = Time.Now;

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

        public void Update() {
            Vector2 oldValue = Value;
            Value = _getter();
            if(Value != oldValue) {
                LastChangedTime = Time.Now;
                Change = Value - oldValue;
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