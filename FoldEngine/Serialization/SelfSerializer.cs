using System;

namespace FoldEngine.Serialization {
    public interface ISelfSerializer : ISerializer {
        void Serialize(SaveOperation writer);
        void Deserialize(LoadOperation reader);
    }
}