using System;
using System.Collections.Generic;

namespace EntryProject.Util {
    public class ObjectPoolCollection<T> {
        private Dictionary<Type, IObjectPool> _pools;

        private ObjectPool<TS> GetPool<TS>() where TS : T, new() {
            if(_pools == null) _pools = new Dictionary<Type, IObjectPool>();
            
            Type type = typeof(TS);
            
            if(!_pools.ContainsKey(type)) {
                ObjectPool<TS> newPool = new ObjectPool<TS>();
                _pools[type] = newPool;
                return newPool;
            }

            return _pools[type] as ObjectPool<TS>;
        }

        public TS Claim<TS>() where TS : T, new() {
            return GetPool<TS>().Claim() is TS ts ? ts : default;
        }

        public void FreeAll() {
            if(_pools != null) {
                foreach(IObjectPool pool in _pools.Values) {
                    pool.FreeAll();
                }
            }
        }
    }
}