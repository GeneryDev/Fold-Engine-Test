﻿using System;
using System.Reflection;
using FoldEngine.Scenes;
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
        public static void Deserialize<T>(ComponentSet componentSet, long entityId, LoadOperation reader) where T : struct {
            reader.ReadCompound(c => {
                foreach(FieldInfo fieldInfo in typeof(T).GetFields()) {
                    if(c.HasMember(fieldInfo.Name)) {
                        c.StartReadMember(fieldInfo.Name);
                        object value = reader.Read(fieldInfo.FieldType);

                        if(reader.Options.Has(DeserializeRemapIds.Instance)
                           && value is long id
                           && fieldInfo.GetCustomAttribute<EntityIdAttribute>() != null
                           && id != -1
                        ) {
                            value = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(id);
                        }

                        componentSet.SetFieldValue(entityId, fieldInfo, value);
                        // Console.WriteLine($"Set field {fieldInfo.Name} to value {value}");
                    }
                }
            });
        }
    }
}