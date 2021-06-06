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
        private Func<float> x;

        public float this[int axis] {
            get {
                if(axis == 0) return x();
                throw new ArgumentException(nameof(axis));
            }
        }

        public int Grade => 1;
        
        public float MagnitudeSqr => x();
        public float Magnitude => (float) Math.Sqrt(MagnitudeSqr);

        public AnalogInfo1(Func<float> x) {
            this.x = x;
        }

        public static implicit operator float(AnalogInfo1 info) {
            return info.x();
        }
    }

    public class AnalogInfo2 : IAnalogInfo {
        private Func<Vector2> _vec;

        public float this[int axis] {
            get {
                switch(axis) {
                    case 0:
                        return _vec().X;
                    case 1:
                        return _vec().Y;
                    default:
                        throw new ArgumentException(nameof(axis));
                }
            }
        }

        public int Grade => 2;
        public float MagnitudeSqr => _vec().LengthSquared();
        public float Magnitude => _vec().Length();

        public AnalogInfo2(Func<Vector2> vec) {
            this._vec = vec;
        }

        public static implicit operator Vector2(AnalogInfo2 info) {
            return info._vec();
        }
    }
}