using System;
using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Components;

public static class ComponentSerializer
{
    public static void Serialize<T>(T component, SaveOperation writer, bool useCustomSerializers = true) where T : struct
    {
        if (useCustomSerializers)
        {
            foreach (var serializer in writer.SerializerSuite.ComponentSerializers)
            {
                if (!serializer.HandlesComponentType(typeof(T))) continue;
                if (serializer.Serialize(component, writer)) return;
            }
        }
        
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

    public static void Deserialize<T>(ComponentSet componentSet, long entityId, LoadOperation reader, bool useCustomSerializers = true)
        where T : struct
    {
        if (useCustomSerializers)
        {
            foreach (var serializer in reader.SerializerSuite.ComponentSerializers)
            {
                if (!serializer.HandlesComponentType(typeof(T))) continue;
                if (serializer.Deserialize(componentSet, entityId, reader)) return;
            }
        }
        
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
                    return;
                }
                else if (fieldInfo.GetCustomAttribute<FormerlySerializedAs>() != null)
                {
                    foreach (FormerlySerializedAs attr in fieldInfo.GetCustomAttributes<FormerlySerializedAs>())
                        if (m.Name == attr.FormerName)
                        {
                            object value = DeserializeComponentField(fieldInfo, reader);
                            componentSet.SetFieldValue(entityId, fieldInfo, value);
                            return;
                        }
                }
            }
            m.Skip();
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

public class CustomComponentSerializer
{
    public virtual bool HandlesComponentType(Type type)
    {
        return false;
    }
    
    public virtual void ScenePreSerialize(Scene scene, SaveOperation writer)
    {
    }
    public virtual bool Serialize(object component, SaveOperation writer)
    {
        return false;
    }
    public virtual void ScenePostSerialize(Scene scene, SaveOperation writer)
    {
    }

    public virtual void ScenePreDeserialize(Scene scene, LoadOperation reader)
    {
    }
    public virtual bool Deserialize(ComponentSet componentSet, long entityId, LoadOperation reader)
    {
        return false;
    }
    public virtual void ScenePostDeserialize(Scene scene, LoadOperation reader)
    {
    }
}