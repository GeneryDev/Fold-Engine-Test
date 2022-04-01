using System;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Resources {
    public class GenericSerializer {
        public static void Serialize(object obj, SaveOperation writer) {
            if(obj is ISelfSerializer serializer) {
                serializer.Serialize(writer);
                return;
            }
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(FieldInfo fieldInfo in obj.GetType().GetFields()) {
                    if(fieldInfo.IsStatic) continue;
                    if(fieldInfo.GetCustomAttribute<DoNotSerialize>() != null) continue;
                    
                    object value = fieldInfo.GetValue(obj);
                    if(value != null) {
                        c.WriteMember(fieldInfo.Name, value);
                    }
                    // Console.WriteLine($"{fieldInfo.Name} = {fieldInfo.GetValue(obj)}");
                }
            });
        }
        public static T Deserialize<T>(T obj, LoadOperation reader) {
            if(obj is ISelfSerializer serializer) {
                serializer.Deserialize(reader);
                return obj;
            }

            object boxed = obj;
            reader.ReadCompound(c => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    if(fieldInfo.IsStatic) continue;
                    if(c.HasMember(fieldInfo.Name)) {
                        object value = DeserializeComponentField(fieldInfo.Name, fieldInfo, reader, c);
                        fieldInfo.SetValue(boxed, value);
                        
                    } else if(fieldInfo.GetCustomAttribute<FormerlySerializedAs>() != null) {
                        foreach(FormerlySerializedAs attr in fieldInfo.GetCustomAttributes<FormerlySerializedAs>()) {
                            if(attr.FormerName != null && c.HasMember(attr.FormerName)) {
                                object value = DeserializeComponentField(attr.FormerName, fieldInfo, reader, c);
                                fieldInfo.SetValue(boxed, value);
                                break;
                            }
                        }
                    }
                }
            });
            return (T) boxed;
        }
        
        private static object DeserializeComponentField(string name, FieldInfo fieldInfo, LoadOperation reader, LoadOperation.Compound c) {
            c.StartReadMember(name);
            object value = reader.Read(fieldInfo.FieldType);
        
            if(reader.Options.Has(DeserializeRemapIds.Instance)
               && value is long id
               && fieldInfo.GetCustomAttribute<EntityIdAttribute>() != null
               && id != -1
            ) {
                value = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(id);
            }
            
            // Console.WriteLine($"Set field {fieldInfo.Name} to value {value}");
        
            return value;
        }
    }
}