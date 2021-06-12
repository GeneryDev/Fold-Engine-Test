using System;

namespace FoldEngine.Serialization {
    public interface ISerializer {
        Type WorkingType { get; }
    }
    public interface ISerializer<T> : ISerializer {

        void Serialize(T t, SaveOperation writer);
        T Deserialize(LoadOperation reader);
    }
}