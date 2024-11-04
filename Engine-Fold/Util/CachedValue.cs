namespace EntryProject.Util {
    public struct CachedValue<T> {
        private T _value;
        public int Generation;

        public T Get(int generation) {
            if(generation != Generation) {
                _value = default;
                Generation = generation;
            }

            return _value;
        }

        public void Set(T value, int generation) {
            _value = value;
            Generation = generation;
        }

        public bool IsValid(int generation) {
            return Generation == generation;
        }
    }
}