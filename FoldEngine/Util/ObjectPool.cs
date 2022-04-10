using System;
using System.Collections.Generic;

namespace EntryProject.Util {
    public interface IObjectPool {
        int ObjectCount { get; }

        void FreeAll();
        object ClaimObject();
        void FreeObject(object o);
    }

    public class ObjectPool<T> : IObjectPool where T : new() {
        private readonly List<T> _objects = new List<T>();
        private int _freeIndex;

        public int ObjectCount => _objects.Count;

        public void FreeAll() {
            _freeIndex = 0;
        }

        public object ClaimObject() {
            return Claim();
        }

        public void FreeObject(object o) {
            if(o is T t) Free(t);
            else throw new ArgumentException("Given object is not of the pool's object type");
        }

        public T Claim() {
            if(_freeIndex >= _objects.Count) {
                var t = new T();
                if(t is IPooledObject pooledT) {
                    pooledT.Pool = this;
                }

                _objects.Add(t);
            }

            return _objects[_freeIndex++];
        }

        public void Free(T t) {
            int index = _objects.IndexOf(t);
            if(index < 0) throw new ArgumentException("Given object is not in the object pool");
            // If object is already free, return early.
            if(index >= _freeIndex) return;
            // If object is the last one returned, free it in place.
            if(index == _freeIndex - 1) {
                _freeIndex--;
                return;
            }

            // Swap the last object returned with the object being freed, and decrement _freeIndex
            _objects[index] = _objects[_freeIndex - 1];
            _objects[_freeIndex - 1] = t;
            _freeIndex--;
        }
    }

    public interface IPooledObject {
        IObjectPool Pool { get; set; }
    }

    public struct PooledValue<T> where T : IPooledObject {
        private T _value;

        public T Value {
            get => _value;
            set {
                _value?.Free();
                _value = value;
            }
        }

        public void Free() {
            Value = default;
        }
    }

    public static class ObjectPoolDefaults {
        public static void Free(this IPooledObject obj) {
            obj.Pool.FreeObject(obj);
        }
    }
}