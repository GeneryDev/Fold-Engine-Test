using System.Collections.Generic;

namespace EntryProject.Util {
    public class FieldCollection {
        private Dictionary<IField, object> _dict;

        public T Get<T>(Field<T> field) {
            if(_dict?.ContainsKey(field) ?? false) return _dict[field] is T t ? t : default;
            return default;
        }

        public bool Has<T>(Field<T> field) {
            return _dict?.ContainsKey(field) ?? false;
        }

        public bool Has(IField field) {
            return _dict?.ContainsKey(field) ?? false;
        }

        public void Set<T>(Field<T> field, T value) {
            if(_dict == null) _dict = new Dictionary<IField, object>();
            _dict[field] = value;
        }

        public delegate void Configurator(FieldCollection options);
    }

    public interface IField { }

    public class Field<T> : IField { }
}