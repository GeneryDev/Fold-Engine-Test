using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Serialization.Serializers;

namespace FoldEngine.Serialization;

public class SerializerSuite
{
    public static readonly SerializerSuite Instance = new SerializerSuite();


    private readonly Dictionary<Type, ISerializer> _serializers = new Dictionary<Type, ISerializer>();

    public List<CustomComponentSerializer> ComponentSerializers { get; } = new();

    public SerializerSuite()
    {
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

        AddSerializer(new PointSerializer());
        AddSerializer(new Vector2Serializer());
        AddSerializer(new Vector3Serializer());
        AddSerializer(new Vector4Serializer());
        AddSerializer(new ColorSerializer());
        AddSerializer(new MatrixSerializer());
        AddSerializer(new LRTBSerializer());

        AddComponentSerializer(new HierarchicalSerializer());
    }

    public SerializerSuite AddSerializer(ISerializer serializer)
    {
        _serializers[serializer.WorkingType] = serializer;
        return this;
    }

    public SerializerSuite AddComponentSerializer(CustomComponentSerializer serializer)
    {
        ComponentSerializers.Add(serializer);
        return this;
    }


    private ISerializer GetSerializer(Type type)
    {
        if (!_serializers.ContainsKey(type)) throw new ArgumentException($"No serializer available for type {type}");
        return _serializers[type];
    }

    private Serializer<T> GetSerializer<T>()
    {
        if (!_serializers.ContainsKey(typeof(T)))
            throw new ArgumentException($"No serializer available for type {typeof(T)}");
        return (Serializer<T>)_serializers[typeof(T)];
    }

    private Serializer<List<T>> GetListSerializer<T>()
    {
        if (!_serializers.ContainsKey(typeof(List<T>))) _serializers[typeof(List<T>)] = new ListSerializer<T>();
        return (Serializer<List<T>>)_serializers[typeof(List<T>)];
    }


    public void Write(object value, SaveOperation writer)
    {
        if (value.GetType().IsEnum)
            writer.Write(value.ToString());
        else
            GetSerializer(value.GetType()).SerializeObject(value, writer);
    }

    public void Write<T>(T element, SaveOperation writer)
    {
        if (typeof(T).GetCustomAttribute(typeof(GenericSerializable)) != null)
            GenericSerializer.Serialize(element, writer);
        else
            GetSerializer<T>().Serialize(element, writer);
    }

    public void Write<T>(List<T> element, SaveOperation writer)
    {
        GetListSerializer<T>().Serialize(element, writer);
    }

    public void Write(ISelfSerializer element, SaveOperation writer)
    {
        element.Serialize(writer);
    }


    public T Read<T>(LoadOperation reader)
    {
        return GetSerializer<T>().Deserialize(reader);
    }

    public object Read(Type type, LoadOperation reader)
    {
        if (type.IsEnum)
        {
            string enumValueName = reader.ReadString();
            foreach (object enumValue in type.GetEnumValues())
                if (enumValueName == type.GetEnumName(enumValue))
                    return enumValue;
            throw new FormatException($"Invalid enum value name '{enumValueName}'");
        }

        return GetSerializer(type).DeserializeObject(reader);
    }

    public List<T> ReadList<T>(LoadOperation reader)
    {
        return GetListSerializer<T>().Deserialize(reader);
    }
}