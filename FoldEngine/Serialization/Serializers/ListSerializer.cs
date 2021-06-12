using System;
using System.Collections.Generic;

namespace FoldEngine.Serialization {
    public class ListSerializer<T> : Serializer<List<T>> {
        public override Type WorkingType => typeof(T);

        public override void Serialize(List<T> t, SaveOperation writer) {
            writer.Write(t.Count);
            foreach(T element in t) {
                writer.Write(element);
            }
        }

        public override List<T> Deserialize(LoadOperation reader) {
            int length = reader.ReadInt32();
            List<T> list = new List<T>(length);
            for(int i = 0; i < length; i++) {
                list[i] = reader.Read<T>();
            }

            return list;
        }
    }
}