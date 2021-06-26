using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Shard.Util;

namespace FoldEngine.Editor.Views {
    public class EditorInspectorView : EditorView {
        public override string Icon => "editor:info";
        public override string Name => "Inspector";

        private long _id = -1;

        private static readonly StringBuilder StringBuilder = new StringBuilder();
        
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            if(_id != -1) {
                ContentPanel.Label(Scene.Components.GetComponent<EntityName>(_id).Name, 14).TextAlignment(-1).Icon(renderer.Textures["editor:cube"]);
                ContentPanel.Label($"ID: {_id}", 7).TextAlignment(-1);
                ContentPanel.Spacing(12);

                foreach(ComponentSet set in Scene.Components.Sets.Values) {
                    // if(set.ComponentType == typeof(EntityName)) continue;
                    if(set.Has((int) _id)) {
                        ComponentInfo componentInfo = ComponentInfo.Get(set.ComponentType);
                        if(componentInfo.HideInInspector) continue;
                        
                        ContentPanel.Separator();
                        

                        ContentPanel.Label(componentInfo.Name, 14).TextAlignment(-1);
                        
                        foreach(ComponentMember member in componentInfo.Members) {
                            object value = set.GetFieldValue((int) _id, member.FieldInfo);
                            ContentPanel
                                .Label(
                                    StringBuilder
                                        .Clear()
                                        .Append(member.Name)
                                        .Append(StringUtil.Repeat(" ", Math.Max(0, 32 - member.Name.Length)))
                                        .Append(value)
                                        .ToString(),
                                    9)
                                .TextAlignment(-1)
                                .UseTextCache(false);
                            if(member.FieldInfo.FieldType == typeof(bool)) {
                                ContentPanel.Button(value.ToString(), 9).LeftAction<TestAction>().Id(_id).FieldInfo(member.FieldInfo).ComponentSet(set);
                            }
                            ContentPanel.Spacing(5);
                        }
                    }
                }
            }
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        }

        public void SetEntity(long id) {
            _id = id;
        }
    }
    
    public class TestAction : IGuiAction {
        private long _id;
        private FieldInfo _fieldInfo;
        private ComponentSet _set;

        public TestAction Id(long id) {
            _id = id;
            return this;
        }
        
        public TestAction FieldInfo(FieldInfo fieldInfo) {
            _fieldInfo = fieldInfo;
            return this;
        }

        public TestAction ComponentSet(ComponentSet set) {
            _set = set;
            return this;
        }

        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            bool oldValue = (bool) _set.GetFieldValue((int) _id, _fieldInfo);
            bool newValue = !oldValue;
            ((EditorEnvironment) element.Parent.Environment).TransactionManager.InsertTransaction(new SetComponentFieldTransaction() {
                ComponentType = _set.ComponentType,
                EntityId = _id,
                FieldInfo = _fieldInfo,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
    }
}