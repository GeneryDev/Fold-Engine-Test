using System;

namespace FoldEngine.Serialization {
    public class StringSerializer : ISerializer<string> {
        public Type WorkingType => typeof(string);

        public void Serialize(string t, SaveOperation writer) {
            writer.Write(t);
        }

        public string Deserialize(LoadOperation reader) {
            return reader.ReadString();
        }
    }
}