using System;
using FoldEngine.Resources;

namespace FoldEngine.Serialization {
    public class StringSerializer : Serializer<string> {
        public override Type WorkingType => typeof(string);

        public override void Serialize(string t, SaveOperation writer) {
            writer.Write(t);
        }

        public override string Deserialize(LoadOperation reader) {
            return reader.ReadString();
        }
    }

    public class ResourceIdentifierSerializer : Serializer<ResourceIdentifier> {
        public override Type WorkingType => typeof(ResourceIdentifier);

        public override void Serialize(ResourceIdentifier t, SaveOperation writer) {
            writer.Write(t.Identifier ?? "");
        }

        public override ResourceIdentifier Deserialize(LoadOperation reader) {
            string identifier = reader.ReadString();
            if(identifier.Length == 0) identifier = null;
            return new ResourceIdentifier(identifier);
        }
    }
}