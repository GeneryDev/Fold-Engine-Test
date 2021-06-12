using System;
using System.Collections.Generic;

namespace FoldEngine.Serialization {
    public class SerializerSuite {
        public static readonly SerializerSuite Instance = new SerializerSuite();
        
        
        private Dictionary<Type, ISerializer> Serializers = new Dictionary<Type, ISerializer>();

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
        }

        public SerializerSuite AddSerializer(ISerializer serializer) {
            Serializers[serializer.WorkingType] = serializer;
            return this;
        }
        

        private ISerializer<T> GetSerializer<T>() {
            return (ISerializer<T>) Serializers[typeof(T)];
        }
        private ISerializer<List<T>> GetListSerializer<T>() {
            if(!Serializers.ContainsKey(typeof(List<T>))) {
                Serializers[typeof(List<T>)] = new ListSerializer<T>();
            }
            return (ISerializer<List<T>>) Serializers[typeof(List<T>)];
        }
        

        public void Write<T>(T element, SaveOperation writer) {
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
        public List<T> ReadList<T>(LoadOperation reader) {
            return GetListSerializer<T>().Deserialize(reader);
        }
    }
}