using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Editor.Inspector;

namespace FoldEngine.Editor.Views {
    public class ComponentInfo {
        private static readonly Dictionary<Type, ComponentInfo> _allInfos = new Dictionary<Type, ComponentInfo>();
        
        public Type ComponentType;
        public string Name;
        public readonly List<ComponentMember> Members = new List<ComponentMember>();
        public bool HideInInspector = false;

        private ComponentInfo(Type componentType) {
            ComponentType = componentType;

            if(componentType.GetCustomAttribute<NameAttribute>() is NameAttribute nameAttribute) {
                Name = nameAttribute.Name;
            } else {
                Name = componentType.Name;
            }

            HideInInspector = componentType.GetCustomAttribute<HideInInspector>() != null;

            if(!HideInInspector) {
                foreach(FieldInfo fieldInfo in componentType.GetFields()) {
                    if(fieldInfo.GetCustomAttribute<HideInInspector>() != null) continue;
                    Members.Add(new ComponentMember(fieldInfo));
                }
            }
        }

        public static ComponentInfo Get(Type type) {
            if(!_allInfos.ContainsKey(type)) {
                return _allInfos[type] = new ComponentInfo(type);
            }
            return _allInfos[type];
        }

        public static ComponentInfo Get<T>() where T : struct {
            return Get(typeof(T));
        }
    }

    public class ComponentMember {
        public string Name;
        public FieldInfo FieldInfo;
        
        public ComponentMember(FieldInfo fieldInfo) {
            FieldInfo = fieldInfo;
            if(fieldInfo.GetCustomAttribute<NameAttribute>() is NameAttribute nameAttribute) {
                Name = nameAttribute.Name;
            } else {
                Name = fieldInfo.Name;
            }
        }
    }
}