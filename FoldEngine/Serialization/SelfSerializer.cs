using System;

namespace FoldEngine.Serialization {
    public interface ISelfSerializer {
        void Serialize(SaveOperation writer);
        void Deserialize(LoadOperation reader);
    }
}