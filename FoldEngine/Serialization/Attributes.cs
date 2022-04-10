using System;

namespace FoldEngine.Serialization {
    public sealed class GenericSerializable : Attribute { }

    public sealed class DoNotSerialize : Attribute { }

    public sealed class FormerlySerializedAs : Attribute {
        public string FormerName;

        public FormerlySerializedAs(string formerName) {
            FormerName = formerName;
        }
    }
}