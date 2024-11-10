using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace FoldEngine.Util.JsonSerialization;

public class JsonDeserializerNode
{
    protected const bool WrapExceptions = true;

    protected string Key;
    protected JsonDeserializerNode Parent;

    public string Path
    {
        get
        {
            JsonDeserializerNode current = this;
            var fullPath = new StringBuilder();

            while (current != null)
            {
                fullPath.Insert(0, current.Key);
                current = current.Parent;
                if (current != null) fullPath.Insert(0, " | ");
            }

            return fullPath.ToString();
        }
    }

    // protected string GetOptionsAsString(options)


    protected object SanitizeBeforeReturn(object value, string key)
    {
        switch (value)
        {
            case JObject valueObj:
                return new JsonDeserializerObject(key, valueObj, this);
            case JArray valueList:
                return new JsonDeserializerArray(key, valueList, this);
            default:
                return value;
        }
    }
}

public class JsonDeserializerObject : JsonDeserializerNode
{
    private readonly JObject _obj;

    public JsonDeserializerObject(string key, JObject obj, JsonDeserializerNode parent = null)
    {
        Key = key;
        _obj = obj;
        Parent = parent;
    }

    public JsonDeserializerObject GetObject(string key, bool nullable = false)
    {
        return Get<JsonDeserializerObject>(key, nullable);
    }

    public JsonDeserializerArray GetArray(string key, bool nullable = false)
    {
        return Get<JsonDeserializerArray>(key, nullable);
    }

    public T Get<T>(string key, bool nullable = false, T defaultValue = default)
    {
        Type type = typeof(T);

        object value = _obj.ContainsKey(key) ? _obj[key] : null;
        if (value is JValue tokenValue) value = tokenValue.Value;

        if (type == typeof(int) && value is long) value = (int)(long)value;
        if (type == typeof(float) && value is int) value = (float)(int)value;
        if (type == typeof(double) && value is int) value = (double)(int)value;

        switch (value)
        {
            case null when nullable:
                return defaultValue;
            case null:
                throw new JsonSchemaException($"{Path} | {key};\nMissing key '{key}'; Expected type: {type}");
            case JObject _ when type == typeof(JsonDeserializerObject):
            case JArray _ when type == typeof(JsonDeserializerArray):
                return (T)SanitizeBeforeReturn(value, key);
            case T t:
                return t;
            default:
                throw new JsonSchemaException(
                    $"{Path} | {key};\nInvalid child option type '{value.GetType()}'; Allowed type: {type}");
        }
    }

    public object Branch(string key, JsonBranches branches)
    {
        object value = _obj.ContainsKey(key) ? _obj[key] : null;

        if (value is JValue tokenValue) value = tokenValue.Value;

        switch (value)
        {
            case null when branches.NullConsumer != null:
                branches.NullConsumer();
                return default;
            case null:
                throw new JsonSchemaException(
                    $"{Path} | {key};\nMissing key '{key}'; Expected types: {branches.AllTypes}");
            default:
            {
                try
                {
                    for (int i = 0; i < branches.Types.Count; i++)
                    {
                        Type type = branches.Types[i];
                        Func<object, object> consumer = branches.Consumers[i];
                        switch (value)
                        {
                            case JObject _ when type == typeof(JsonDeserializerObject):
                            case JArray _ when type == typeof(JsonDeserializerArray):
                                value = SanitizeBeforeReturn(value, key);
                                return consumer(value);
                            default:
                            {
                                if (type.IsInstanceOfType(value)) return consumer(value);

                                break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    if (!WrapExceptions || x is JsonSchemaException)
                        throw x;
                    throw new JsonSchemaException($"{Path} | {key};\n{x.Message}");
                }

                throw new JsonSchemaException(
                    $"{Path} | {key};\nInvalid child option type '{value.GetType()}'; Allowed types: {branches.AllTypes}");
            }
        }
    }

    public bool ContainsKey(string key)
    {
        return _obj.ContainsKey(key);
    }
}

public class JsonDeserializerArray : JsonDeserializerNode
{
    private readonly JArray _arr;

    public JsonDeserializerArray(string key, JArray arr, JsonDeserializerNode parent = null)
    {
        Key = key;
        _arr = arr;
        Parent = parent;
    }

    public int Length => _arr.Count;
    public int Count => _arr.Count;

    public JsonDeserializerObject GetObject(int index, bool nullable = false)
    {
        return Get<JsonDeserializerObject>(index, nullable);
    }

    public JsonDeserializerArray GetArray(int index, bool nullable = false)
    {
        return Get<JsonDeserializerArray>(index, nullable);
    }

    public T Get<T>(int index, bool nullable = false, T defaultValue = default)
    {
        Type type = typeof(T);

        object value = index >= 0 && index < _arr.Count ? _arr[index] : null;

        if (value is JValue tokenValue) value = tokenValue.Value;

        if (type == typeof(double) && value is int) value = (double)(int)value;

        switch (value)
        {
            case null when nullable:
                return defaultValue;
            case null:
                throw new JsonSchemaException($"{Path} | {index};\nMissing index '{index}'; Expected type: {type}");
            case JObject _ when type == typeof(JsonDeserializerObject):
            case JArray _ when type == typeof(JsonDeserializerArray):
                return (T)SanitizeBeforeReturn(value, index.ToString());
            case T t:
                return t;
            default:
                throw new JsonSchemaException(
                    $"{Path} | {index};\nInvalid child option type '{value.GetType()}'; Allowed type: {type}");
        }
    }

    public object Branch(int index, JsonBranches branches)
    {
        object value = index >= 0 && index < _arr.Count ? _arr[index] : null;

        if (value is JValue tokenValue) value = tokenValue.Value;

        switch (value)
        {
            case null when branches.NullConsumer != null:
                branches.NullConsumer();
                return default;
            case null:
                throw new JsonSchemaException(
                    $"{Path} | {index};\nMissing index '{index}'; Expected types: {branches.AllTypes}");
            default:
            {
                try
                {
                    for (int i = 0; i < branches.Types.Count; i++)
                    {
                        Type type = branches.Types[i];
                        Func<object, object> consumer = branches.Consumers[i];
                        switch (value)
                        {
                            case JObject _ when type == typeof(JsonDeserializerObject):
                            case JArray _ when type == typeof(JsonDeserializerArray):
                                value = SanitizeBeforeReturn(value, index.ToString());
                                return consumer(value);
                            default:
                            {
                                if (type.IsInstanceOfType(value)) return consumer(value);

                                break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    if (!WrapExceptions || x is JsonSchemaException)
                        throw x;
                    throw new JsonSchemaException($"{Path} | {index};\n{x.Message}");
                }

                throw new JsonSchemaException(
                    $"{Path} | {index};\nInvalid child option type '{value.GetType()}'; Allowed types: {branches.AllTypes}");
            }
        }
    }


    public void Iterate(JsonBranches branches)
    {
        for (int i = 0; i < Length; i++) Branch(i, branches);
    }
}

public class JsonDeserializerRoot : JsonDeserializerNode
{
    private readonly JToken _root;

    public JsonDeserializerRoot(string key, JToken root)
    {
        Key = key;
        _root = root;
    }

    public static JsonDeserializerRoot NewFromFile(string path)
    {
        return new JsonDeserializerRoot(path, JToken.Parse(File.OpenText(path).ReadToEnd()));
    }

    public JsonDeserializerObject AsObject()
    {
        switch (_root)
        {
            case JObject rootObj:
                return new JsonDeserializerObject("<root>", rootObj, this);
            default:
                throw new JsonSchemaException($"{Path};\nInvalid root type '{_root.Type}'; Expected object.");
        }
    }

    public JsonDeserializerArray AsArray()
    {
        switch (_root)
        {
            case JArray rootArr:
                return new JsonDeserializerArray("<root>", rootArr, this);
            default:
                throw new JsonSchemaException($"{Path};\nInvalid root type '{_root.Type}'; Expected array.");
        }
    }

    public static explicit operator JsonDeserializerObject(JsonDeserializerRoot root)
    {
        return root.AsObject();
    }

    public static explicit operator JsonDeserializerArray(JsonDeserializerRoot root)
    {
        return root.AsArray();
    }
}

public class JsonBranches
{
    protected internal List<Func<object, object>> Consumers = new List<Func<object, object>>();
    protected internal Func<object> NullConsumer;
    protected internal List<Type> Types = new List<Type>();

    public string AllTypes
    {
        get
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Types.Count; i++)
            {
                sb.Append(Types[i].Name);
                if (i < Types.Count - 1) sb.Append(", ");
            }

            return sb.ToString();
        }
    }

    public JsonBranches Add<T>(Func<T, object> consumer)
    {
        Types.Add(typeof(T));
        Consumers.Add(a => consumer((T)a));
        return this;
    }

    public JsonBranches Null(Func<object> nullConsumer)
    {
        NullConsumer = nullConsumer;
        return this;
    }
}

[Serializable]
public class JsonSchemaException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public JsonSchemaException()
    {
    }

    public JsonSchemaException(string message) : base(message)
    {
    }

    public JsonSchemaException(string message, Exception inner) : base(message, inner)
    {
    }

    protected JsonSchemaException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}