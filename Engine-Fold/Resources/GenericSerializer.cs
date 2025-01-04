using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Resources;

public static class GenericSerializer
{
    public static void Serialize(object obj, SaveOperation writer)
    {
        if (obj is ISelfSerializer serializer)
        {
            serializer.Serialize(writer);
            return;
        }

        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
            {
                if (fieldInfo.IsStatic) continue;
                if (fieldInfo.GetCustomAttribute<DoNotSerialize>() != null) continue;

                object value = fieldInfo.GetValue(obj);
                if (value != null) c.WriteMember(fieldInfo.Name, value);
                // Console.WriteLine($"{fieldInfo.Name} = {fieldInfo.GetValue(obj)}");
            }
        });
    }

    public static T Deserialize<T>(T obj, LoadOperation reader)
    {
        if (obj is ISelfSerializer serializer)
        {
            serializer.Deserialize(reader);
            return obj;
        }

        object boxed = obj;
        var fieldInfos = obj.GetType().GetFields();
        // TODO optimize, via type.GetField() + support for FormerlySerializedAsAttribute
        reader.ReadCompound(m =>
        {
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic) continue;
                if (m.Name == fieldInfo.Name)
                {
                    object value = DeserializeObjectField(fieldInfo.Name, fieldInfo, reader);
                    fieldInfo.SetValue(boxed, value);
                    return;
                }
                else if (fieldInfo.GetCustomAttribute<FormerlySerializedAs>() != null)
                {
                    foreach (FormerlySerializedAs attr in fieldInfo.GetCustomAttributes<FormerlySerializedAs>())
                        if (attr.FormerName == m.Name)
                        {
                            object value = DeserializeObjectField(attr.FormerName, fieldInfo, reader);
                            fieldInfo.SetValue(boxed, value);
                            return;
                        }
                }
            }
            m.Skip();
        });
        return (T)boxed;
    }

    private static object DeserializeObjectField(
        string name,
        FieldInfo fieldInfo,
        LoadOperation reader)
    {
        object value = reader.Read(fieldInfo.FieldType);

        if (reader.Options.Has(DeserializeRemapIds.Instance)
            && value is long id
            && fieldInfo.GetCustomAttribute<EntityIdAttribute>() != null
            && id != -1
           )
            value = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(id);

        // Console.WriteLine($"Set field {fieldInfo.Name} to value {value}");

        return value;
    }
}