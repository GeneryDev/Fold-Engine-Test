using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Components {
    public static class ComponentSerializer {
        public static void Serialize<T>(T component, SaveOperation writer) where T : struct {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    if(fieldInfo.IsStatic) continue;
                    if(fieldInfo.GetCustomAttribute<DoNotSerialize>() != null) continue;

                    object value = fieldInfo.GetValue(component);
                    if(value != null) c.WriteMember(fieldInfo.Name, value);
                    // Console.WriteLine($"{fieldInfo.Name} = {fieldInfo.GetValue(component)}");
                }
            });
        }

        public static void Deserialize<T>(ComponentSet componentSet, long entityId, LoadOperation reader)
            where T : struct {
            reader.ReadCompound(c => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    if(fieldInfo.IsStatic) continue;
                    if(c.HasMember(fieldInfo.Name)) {
                        object value = DeserializeComponentField(fieldInfo.Name, fieldInfo, reader, c);
                        componentSet.SetFieldValue(entityId, fieldInfo, value);
                    } else if(fieldInfo.GetCustomAttribute<FormerlySerializedAs>() != null) {
                        foreach(FormerlySerializedAs attr in fieldInfo.GetCustomAttributes<FormerlySerializedAs>())
                            if(attr.FormerName != null && c.HasMember(attr.FormerName)) {
                                object value = DeserializeComponentField(attr.FormerName, fieldInfo, reader, c);
                                componentSet.SetFieldValue(entityId, fieldInfo, value);
                                break;
                            }
                    }
                }
            });
        }

        private static object DeserializeComponentField(
            string name,
            FieldInfo fieldInfo,
            LoadOperation reader,
            LoadOperation.Compound c) {
            c.StartReadMember(name);
            object value = reader.Read(fieldInfo.FieldType);

            if(reader.Options.Has(DeserializeRemapIds.Instance)
               && value is long id
               && fieldInfo.GetCustomAttribute<EntityIdAttribute>() != null
               && id != -1
            )
                value = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(id);

            // Console.WriteLine($"Set field {fieldInfo.Name} to value {value}");

            return value;
        }
    }
}