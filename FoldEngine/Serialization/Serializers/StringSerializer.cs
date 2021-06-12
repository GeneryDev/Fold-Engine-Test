using System;

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
}