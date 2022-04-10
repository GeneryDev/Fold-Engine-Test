using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Resources;

namespace FoldEngine.Serialization {
    public class SerializerSuite {
        public static readonly SerializerSuite Instance = new SerializerSuite();


        private readonly Dictionary<Type, ISerializer> Serializers = new Dictionary<Type, ISerializer>();

        public SerializerSuite() {
            AddSerializer(new PrimitiveSerializer<byte>());
            AddSerializer(new PrimitiveSerializer<sbyte>());

            AddSerializer(new PrimitiveSerializer<short>());
            AddSerializer(new PrimitiveSerializer<ushort>());

            AddSerializer(new PrimitiveSerializer<int>());
            AddSerializer(new PrimitiveSerializer<uint>());

            AddSerializer(new PrimitiveSerializer<long>());
            AddSerializer(new PrimitiveSerializer<ulong>());

            AddSerializer(new PrimitiveSerializer<float>());
            AddSerializer(new PrimitiveSerializer<double>());
            AddSerializer(new PrimitiveSerializer<decimal>());

            AddSerializer(new PrimitiveSerializer<bool>());

            AddSerializer(new StringSerializer());
            AddSerializer(new ResourceIdentifierSerializer());

            AddSerializer(new Vector2Serializer());
            AddSerializer(new Vector3Serializer());
            AddSerializer(new Vector4Serializer());
            AddSerializer(new ColorSerializer());
            AddSerializer(new MatrixSerializer());
        }

        public SerializerSuite AddSerializer(ISerializer serializer) {
            Serializers[serializer.WorkingType] = serializer;
            return this;
        }


        private ISerializer GetSerializer(Type type) {
            if(!Serializers.ContainsKey(type)) throw new ArgumentException($"No serializer available for type {type}");
            return Serializers[type];
        }

        private Serializer<T> GetSerializer<T>() {
            if(!Serializers.ContainsKey(typeof(T)))
                throw new ArgumentException($"No serializer available for type {typeof(T)}");
            return (Serializer<T>) Serializers[typeof(T)];
        }

        private Serializer<List<T>> GetListSerializer<T>() {
            if(!Serializers.ContainsKey(typeof(List<T>))) Serializers[typeof(List<T>)] = new ListSerializer<T>();
            return (Serializer<List<T>>) Serializers[typeof(List<T>)];
        }


        public void Write(object value, SaveOperation writer) {
            if(value.GetType().IsEnum)
                writer.Write(value.ToString());
            else
                GetSerializer(value.GetType()).SerializeObject(value, writer);
        }

        public void Write<T>(T element, SaveOperation writer) {
            if(typeof(T).GetCustomAttribute(typeof(GenericSerializable)) != null)
                GenericSerializer.Serialize(element, writer);
            else
                GetSerializer<T>().Serialize(element, writer);
        }

        public void Write<T>(List<T> element, SaveOperation writer) {
            GetListSerializer<T>().Serialize(element, writer);
        }

        public void Write(ISelfSerializer element, SaveOperation writer) {
            element.Serialize(writer);
        }


        public T Read<T>(LoadOperation reader) {
            return GetSerializer<T>().Deserialize(reader);
        }

        public object Read(Type type, LoadOperation reader) {
            if(type.IsEnum) {
                string enumValueName = reader.ReadString();
                foreach(object enumValue in type.GetEnumValues())
                    if(enumValueName == type.GetEnumName(enumValue))
                        return enumValue;
                throw new FormatException($"Invalid enum value name '{enumValueName}'");
            }

            return GetSerializer(type).DeserializeObject(reader);
        }

        public List<T> ReadList<T>(LoadOperation reader) {
            return GetListSerializer<T>().Deserialize(reader);
        }
    }
}