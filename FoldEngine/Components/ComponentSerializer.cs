using System;
using System.Reflection;
using FoldEngine.Serialization;

namespace FoldEngine.Components {
    public class ComponentSerializer {
        public static void Serialize<T>(T component, SaveOperation writer) where T : struct {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    object value = fieldInfo.GetValue(component);
                    if(value != null) {
                        c.WriteMember(fieldInfo.Name, value);
                    }
                    // Console.WriteLine($"{fieldInfo.Name} = {fieldInfo.GetValue(component)}");
                }
            });
        }
        public static T Deserialize<T>(object boxed, LoadOperation reader) where T : struct {
            reader.ReadCompound(c => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    if(c.HasMember(fieldInfo.Name)) {
                        c.StartReadMember(fieldInfo.Name);
                        object value = reader.Read(fieldInfo.FieldType);
                        fieldInfo.SetValue(boxed, value);
                    }
                    // Console.WriteLine($"Set field {fieldInfo.Name} to value {value}");
                }
            });
            return (T) boxed;
        }
    }
}