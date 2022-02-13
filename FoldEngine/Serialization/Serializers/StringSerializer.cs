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
    public class ResourceLocationSerializer : Serializer<ResourceLocation> {
        public override Type WorkingType => typeof(ResourceLocation);

        public override void Serialize(ResourceLocation t, SaveOperation writer) {
            writer.Write(t.Identifier ?? "");
        }

        public override ResourceLocation Deserialize(LoadOperation reader) {
            string identifier = reader.ReadString();
            if(identifier.Length == 0) identifier = null;
            return new ResourceLocation(identifier);
        }
    }
}