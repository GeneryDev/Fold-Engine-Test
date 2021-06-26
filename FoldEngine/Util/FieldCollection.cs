using System.Collections.Generic;

namespace EntryProject.Util {
    public class FieldCollection {
        private Dictionary<IField, object> _dict = new Dictionary<IField,object>();

        public T Get<T>(Field<T> field) {
            if(_dict.ContainsKey(field)) {
                return _dict[field] is T t ? t : default;
            }
            return default;
        }

        public bool Has<T>(Field<T> field) {
            return _dict.ContainsKey(field);
        }

        public void Set<T>(Field<T> field, T value) {
            _dict[field] = value;
        }
    }

    public interface IField {
        
    }

    public class Field<T> : IField {
        
    }
}