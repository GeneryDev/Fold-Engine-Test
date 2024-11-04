using System;

namespace FoldEngine.Serialization {
    public class PrimitiveSerializer<T> : Serializer<T> where T : struct {
        public override Type WorkingType => typeof(T);

        public override void Serialize(T t, SaveOperation writer) {
            switch(t) {
                case byte v:
                    writer.Write(v);
                    return;
                case sbyte v:
                    writer.Write(v);
                    return;

                case short v:
                    writer.Write(v);
                    return;
                case ushort v:
                    writer.Write(v);
                    return;

                case int v:
                    writer.Write(v);
                    return;
                case uint v:
                    writer.Write(v);
                    return;

                case long v:
                    writer.Write(v);
                    return;
                case ulong v:
                    writer.Write(v);
                    return;

                case float v:
                    writer.Write(v);
                    return;
                case double v:
                    writer.Write(v);
                    return;
                case decimal v:
                    writer.Write(v);
                    return;

                case bool v:
                    writer.Write(v);
                    return;
                default: throw new ArgumentException(nameof(T));
            }
        }

        public override T Deserialize(LoadOperation reader) {
            switch((T) default) {
                case byte _: return reader.ReadByte() is T b ? b : default;
                case sbyte _: return reader.ReadSByte() is T sb ? sb : default;

                case short _: return reader.ReadInt16() is T s ? s : default;
                case ushort _: return reader.ReadUInt16() is T us ? us : default;

                case int _: return reader.ReadInt32() is T i ? i : default;
                case uint _: return reader.ReadUInt32() is T ui ? ui : default;

                case long _: return reader.ReadInt64() is T l ? l : default;
                case ulong _: return reader.ReadUInt64() is T ul ? ul : default;


                case float _: return reader.ReadSingle() is T f ? f : default;
                case double _: return reader.ReadDouble() is T d ? d : default;
                case decimal _: return reader.ReadDecimal() is T dec ? dec : default;

                case bool _: return reader.ReadBoolean() is T bo ? bo : default;

                default: throw new ArgumentException(nameof(T));
            }
        }
    }
}