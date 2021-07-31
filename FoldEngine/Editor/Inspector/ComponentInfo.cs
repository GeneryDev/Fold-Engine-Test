using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Fields;
using FoldEngine.Editor.Gui.Fields.Text;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui;
using Microsoft.Xna.Framework;

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
        public static Dictionary<Type, InspectorElementProvider> TypeToInspectorElementProvider =
            new Dictionary<Type, InspectorElementProvider>();
        
        public string Name;
        public FieldInfo FieldInfo;
        private InspectorElementProvider _createInspectorElement;

        public ComponentMember(FieldInfo fieldInfo) {
            FieldInfo = fieldInfo;
            if(fieldInfo.GetCustomAttribute<NameAttribute>() is NameAttribute nameAttribute) {
                Name = nameAttribute.Name;
            } else {
                Name = fieldInfo.Name;
            }

            if(TypeToInspectorElementProvider.ContainsKey(fieldInfo.FieldType)) {
                _createInspectorElement = TypeToInspectorElementProvider[fieldInfo.FieldType];
            }
        }
        
        private object _createNextForObject = null;
        private long _createNextForId = -1;

        public ComponentMember ForObject(object obj) {
            _createNextForObject = obj;
            _createNextForId = -1;
            return this;
        }

        public ComponentMember ForEntity(ComponentSet set, long id) {
            _createNextForObject = set;
            _createNextForId = id;
            return this;
        }

        public void CreateInspectorElement(GuiPanel parentPanel, object startingValue) {
            if(_createInspectorElement != null) {
                _createInspectorElement(parentPanel, this, startingValue);
                return;
            } else {
                parentPanel.Label(startingValue?.ToString() ?? "", 9).TextAlignment(-1).UseTextCache(false);
            }
        }
        
        private SetFieldAction CreateAction(GuiPanel panel, int index = 0) {
            SetFieldAction action;
            if(_createNextForId != -1) {
                action = panel.Environment.ActionPool.Claim<SetComponentFieldAction>()
                    .ComponentSet((ComponentSet) _createNextForObject)
                    .Id(_createNextForId);
            } else {
                action = panel.Environment.ActionPool.Claim<SetObjectFieldAction>()
                    .Object(_createNextForObject);
            }
            
            action.FieldInfo(FieldInfo);
            action.Index(0);
            return action;
        }

        public static void SetDefaultInspectorElementProvider<T>(InspectorElementProvider provider) {
            TypeToInspectorElementProvider[typeof(T)] = provider;
        }

        static ComponentMember() {
            // Checkbox
            // bool
            SetDefaultInspectorElementProvider<bool>((parentPanel, member, startingValue) =>
                parentPanel.Element<Checkbox>()
                    .Value((bool) startingValue)
                    .EditedAction(member.CreateAction(parentPanel)));
            
            // Text Field
            // strings and various numeric types
            InspectorElementProvider textFieldProvider = (parentPanel, member, startingValue) => parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth)
                    .Value(startingValue?.ToString() ?? "")
                    .EditedAction(member.CreateAction(parentPanel))
                ;
            SetDefaultInspectorElementProvider<string>(textFieldProvider);
            SetDefaultInspectorElementProvider<int>(textFieldProvider);
            SetDefaultInspectorElementProvider<long>(textFieldProvider);
            SetDefaultInspectorElementProvider<float>(textFieldProvider);
            SetDefaultInspectorElementProvider<double>(textFieldProvider);


            // Multiple Text Fields
            // Vector2
            SetDefaultInspectorElementProvider<Vector2>((parentPanel, member, startingValue) => {
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                    .Value(((Vector2) startingValue).X.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 0));
                
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                    .Value(((Vector2) startingValue).Y.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 1));
            });
            
            // Color
            SetDefaultInspectorElementProvider<Color>((parentPanel, member, startingValue) => {
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).R.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 0));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).G.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 1));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).B.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 2));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).A.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(member.CreateAction(parentPanel, 3));
            });
        }

        public delegate void InspectorElementProvider(GuiPanel parentPanel, ComponentMember member, object startingValue);
    }
}