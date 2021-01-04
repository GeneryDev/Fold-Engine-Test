using System;
using System.Collections;
using System.Collections.Generic;

namespace FoldEngine.Util {
    public class FixedSizeBuffer<T> : IEnumerable<T> {
        private T[] _buffer;
        private int _nextIndex = 0;

        public int Count { get; private set; } = 0;

        public FixedSizeBuffer(int samples) {
            _buffer = new T[samples];
        }

        public void Put(T element) {
            _buffer[_nextIndex] = element;
            Count = Math.Max(_buffer.Length, _nextIndex+1);
            _nextIndex = (_nextIndex + 1) % _buffer.Length;
        }


        public IEnumerator<T> GetEnumerator() {
            for(int i = 0; i < Count; i++) {
                yield return _buffer[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            for(int i = 0; i < Count; i++) {
                yield return _buffer[i];
            }
        }
    }

    public class FixedSizeFloatBuffer : FixedSizeBuffer<float> {
        public FixedSizeFloatBuffer(int samples) : base(samples) {}

        public float Sum() {
            float sum = 0;
            foreach(float sample in this) {
                sum += sample;
            }
            return sum;
        }
        public float Average() {
            float sum = 0;
            foreach(float sample in this) {
                sum += sample;
            }
            return sum / this.Count;
        }
        public float Max() {
            float max = float.NegativeInfinity;
            foreach(float sample in this) {
                if(sample > max) max = sample;
            }
            return max;
        }
        public float Min() {
            float min = float.PositiveInfinity;
            foreach(float sample in this) {
                if(sample < min) min = sample;
            }
            return min;
        }
    }
}