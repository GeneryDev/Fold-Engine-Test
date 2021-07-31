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
                _createInspectorElement(parentPanel, startingValue);
                return;
            }

            CreateInspectorElementForType(parentPanel, startingValue);
        }
        
        

        private SetFieldAction CreateAction(GuiPanel panel, FieldInfo fieldInfo, int index = 0) {
            SetFieldAction action;
            if(_createNextForId != -1) {
                action = panel.Environment.ActionPool.Claim<SetComponentFieldAction>()
                    .ComponentSet((ComponentSet) _createNextForObject)
                    .Id(_createNextForId);
            } else {
                action = panel.Environment.ActionPool.Claim<SetObjectFieldAction>()
                    .Object(_createNextForObject);
            }
            
            action.FieldInfo(fieldInfo);
            action.Index(0);
            return action;
        }

        private void CreateInspectorElementForType(GuiPanel parentPanel, object startingValue) {
            FieldInfo fieldInfo = FieldInfo;
            if(fieldInfo.FieldType == typeof(bool)) {
                parentPanel.Element<Checkbox>()
                    .Value((bool) startingValue)
                    .EditedAction(CreateAction(parentPanel, fieldInfo));
            } else if(fieldInfo.FieldType == typeof(string)
                      || fieldInfo.FieldType == typeof(int)
                      || fieldInfo.FieldType == typeof(long)
                      || fieldInfo.FieldType == typeof(float)
                      || fieldInfo.FieldType == typeof(double)) {
                
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth)
                    .Value(startingValue?.ToString() ?? "")
                    .EditedAction(CreateAction(parentPanel, fieldInfo))
                    ;

            } else if(fieldInfo.FieldType == typeof(Vector2)) {
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                    .Value(((Vector2) startingValue).X.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 0));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                    .Value(((Vector2) startingValue).Y.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 1));
            } else if(fieldInfo.FieldType == typeof(Color)) {
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).R.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 0));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).G.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 1));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).B.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 2));
                parentPanel.Element<TextField>()
                    .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                    .Value(((Color) startingValue).A.ToString(CultureInfo.InvariantCulture))
                    .EditedAction(CreateAction(parentPanel, fieldInfo, 3));
            } else {
                parentPanel.Label(startingValue?.ToString() ?? "", 9).TextAlignment(-1).UseTextCache(false);
            }
        }

        public delegate void InspectorElementProvider(GuiPanel parentPanel, object startingValue);
    }
}