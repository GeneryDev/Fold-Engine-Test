using System;

namespace FoldEngine.Serialization {
    public interface ISerializer {
        Type WorkingType { get; }

        void SerializeObject(object t, SaveOperation writer);
        object DeserializeObject(LoadOperation writer);
    }

    public abstract class Serializer<T> : ISerializer {
        public abstract void Serialize(T t, SaveOperation writer);
        public abstract T Deserialize(LoadOperation reader);

        public void SerializeObject(object value, SaveOperation writer) {
            if(value is T t) Serialize(t, writer);
            else throw new ArgumentException(nameof(value));
        }

        public object DeserializeObject(LoadOperation writer) {
            return Deserialize(writer);
        }
        
        public abstract Type WorkingType { get; }
    }
}