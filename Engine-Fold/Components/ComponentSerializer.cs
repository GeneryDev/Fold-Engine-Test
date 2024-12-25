using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Components;

public static class ComponentSerializer
{
    public static void Serialize<T>(T component, SaveOperation writer) where T : struct
    {
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            foreach (FieldInfo fieldInfo in typeof(T).GetFields())
            {
                if (fieldInfo.IsStatic) continue;
                if (fieldInfo.GetCustomAttribute<DoNotSerialize>() != null) continue;

                object value = fieldInfo.GetValue(component);
                if (value != null) c.WriteMember(fieldInfo.Name, value);
                // Console.WriteLine($"{fieldInfo.Name} = {fieldInfo.GetValue(component)}");
            }
        });
    }

    public static void Deserialize<T>(ComponentSet componentSet, long entityId, LoadOperation reader)
        where T : struct
    {
        // TODO optimize, via type.GetField() + support for FormerlySerializedAsAttribute
        var fieldInfos = typeof(T).GetFields();
        reader.ReadCompound(m =>
        {
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic) continue;
                if (m.Name == fieldInfo.Name)
                {
                    object value = DeserializeComponentField(fieldInfo, reader);
                    componentSet.SetFieldValue(entityId, fieldInfo, value);
                }
                else if (fieldInfo.GetCustomAttribute<FormerlySerializedAs>() != null)
                {
                    foreach (FormerlySerializedAs attr in fieldInfo.GetCustomAttributes<FormerlySerializedAs>())
                        if (m.Name == attr.FormerName)
                        {
                            object value = DeserializeComponentField(fieldInfo, reader);
                            componentSet.SetFieldValue(entityId, fieldInfo, value);
                            break;
                        }
                }
            }
        });
    }

    private static object DeserializeComponentField(FieldInfo fieldInfo,
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