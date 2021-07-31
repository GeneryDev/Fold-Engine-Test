using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Fields;
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
        private object _object = null;

        private static readonly StringBuilder StringBuilder = new StringBuilder();
        
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            if(_id != -1 && Scene.Components.HasComponent<Transform>(_id)) {
                RenderEntityView(renderer);
            } else if(_object != null) {
                RenderObjectView(renderer);
            }
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        }

        private void RenderEntityView(IRenderingUnit renderer) {
            ContentPanel.Label(Scene.Components.GetComponent<EntityName>(_id).Name, 14)
                .TextAlignment(-1)
                .Icon(renderer.Textures["editor:cube"]);
            ContentPanel.Label($"ID: {_id}", 7).TextAlignment(-1);
            ContentPanel.Spacing(12);

            foreach(ComponentSet set in Scene.Components.Sets.Values) {
                // if(set.ComponentType == typeof(EntityName)) continue;
                if(set.Has((int) _id)) {
                    ComponentInfo componentInfo = ComponentInfo.Get(set.ComponentType);
                    if(componentInfo.HideInInspector) continue;

                    ContentPanel.Spacing(5);

                    ContentPanel.Element<ComponentHeader>()
                        .Info(componentInfo)
                        .Id(_id)
                        .ContextMenuAction<ShowComponentMenuAction>()
                        .Info(componentInfo)
                        .Id(_id);

                    // ContentPanel.Label(componentInfo.Name, 14).TextAlignment(-1);

                    foreach(ComponentMember member in componentInfo.Members) {
                        object value = set.GetFieldValue((int) _id, member.FieldInfo);
                        // ContentPanel
                        //     .Label(
                        //         StringBuilder
                        //             .Clear()
                        //             .Append(member.Name)
                        //             .Append(StringUtil.Repeat(" ", Math.Max(0, 32 - member.Name.Length)))
                        //             .Append(value)
                        //             .ToString(),
                        //         9)
                        //     .TextAlignment(-1)
                        //     .UseTextCache(false);

                        ContentPanel.Element<ComponentMemberLabel>().Member(member);

                        member.ForEntity(set, _id).CreateInspectorElement(ContentPanel, value);

                        ContentPanel.Element<ComponentMemberBreak>();
                        // ContentPanel.Spacing(5);
                    }
                }
            }

            ContentPanel.Button("Add Component", 14).LeftAction<ShowAddComponentMenuAction>().Id(_id);
        }

        private void RenderObjectView(IRenderingUnit renderer) {
            ContentPanel.Label(Scene.Components.GetComponent<EntityName>(_id).Name, 14)
                .TextAlignment(-1)
                .Icon(renderer.Textures["editor:cube"]);
            ContentPanel.Label($"ID: {_id}", 7).TextAlignment(-1);
            ContentPanel.Spacing(12);
            
            
            ComponentInfo componentInfo = ComponentInfo.Get(_object.GetType());

            foreach(ComponentMember member in componentInfo.Members) {
                object value = member.FieldInfo.GetValue(_object);

                ContentPanel.Element<ComponentMemberLabel>().Member(member);

                member.ForObject(_object).CreateInspectorElement(ContentPanel, value);

                ContentPanel.Element<ComponentMemberBreak>();
            }
        }

        public void SetEntity(long id) {
            _id = id;
            _object = null;
        }

        public void SetObject(object obj) {
            _object = obj;
            _id = -1;
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

    public class ComponentMemberLabel : GuiLabel {
        public const int LabelWidth = 140;
        
        private ComponentMember _member;

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            FontSize(9);
            TextAlignment(-1);

            UseTextCache(true);
        }

        public ComponentMemberLabel Member(ComponentMember member) {
            _member = member;
            Text(member.Name);
            return this;
        }

        public override void Displace(ref Point layoutPosition) {
            layoutPosition.X += LabelWidth;
        }
    }

    public class ComponentMemberBreak : GuiElement {
        public override void Reset(GuiPanel parent) {
        }

        public override void AdjustSpacing(GuiPanel parent) {
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
        }

        public override void Displace(ref Point layoutPosition) {
            layoutPosition.X = Parent.Bounds.X;
            layoutPosition.Y += 20;
        }
    }

    public class ComponentHeader : GuiLabel {
        private ComponentInfo _info;
        private long _id;

        private PooledValue<IGuiAction> _contextMenuAction;

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            _contextMenuAction.Free();
        }

        public ComponentHeader Info(ComponentInfo info) {
            _info = info;
            Text(info.Name);
            FontSize(14);
            TextAlignment(-1);
            return this;
        }

        public ComponentHeader Id(long id) {
            _id = id;
            return this;
        }
        
        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 8;
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            if(e.Button == MouseEvent.RightButton) {
                _contextMenuAction.Value?.Perform(this, e);
            }
            base.OnMouseReleased(ref e);
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds
            });
            // _textColor = Rollover ? Color.CornflowerBlue : Color.White;
            base.Render(renderer, layer);
        }

        public ComponentHeader ContextMenuAction(IGuiAction action) {
            _contextMenuAction.Value = action;
            return this;
        }

        public T ContextMenuAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _contextMenuAction.Value = action;
            return action;
        }
    }
    
    public class ShowAddComponentMenuAction : IGuiAction {
        private long _id;

        public ShowAddComponentMenuAction Id(long id) {
            _id = id;
            return this;
        }
        
        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            GuiPopupMenu contextMenu = element.Parent.Environment.ContextMenu;
            contextMenu.Reset(e.Position);

            foreach(ComponentSet set in element.Environment.Scene.Components.Sets.Values) {
                if(!set.Has(_id)) contextMenu.Button(set.ComponentType.Name, 9).LeftAction<AddComponentAction>().Id(_id).Type(set.ComponentType);
            }
            
            contextMenu.Show();
        }
    }
    
    public class AddComponentAction : IGuiAction {
        private Type _type;
        private long _id;

        public AddComponentAction Type(Type type) {
            _type = type;
            return this;
        }

        public AddComponentAction Id(long id) {
            _id = id;
            return this;
        }
        
        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            ((EditorEnvironment) element.Environment).TransactionManager.InsertTransaction(new AddComponentTransaction(_type, _id));
        }
    }
    
    public class ShowComponentMenuAction : IGuiAction {
        private ComponentInfo _info;
        private long _id;

        public ShowComponentMenuAction Info(ComponentInfo info) {
            _info = info;
            return this;
        }

        public ShowComponentMenuAction Id(long id) {
            _id = id;
            return this;
        }
        
        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            GuiPopupMenu contextMenu = element.Parent.Environment.ContextMenu;
            contextMenu.Reset(e.Position);

            contextMenu.Button("Remove", 14).LeftAction<RemoveComponentAction>().Id(_id).Type(_info.ComponentType);
            
            contextMenu.Show();
        }
    }
    
    public class RemoveComponentAction : IGuiAction {
        private Type _type;
        private long _id;

        public RemoveComponentAction Type(Type type) {
            _type = type;
            return this;
        }

        public RemoveComponentAction Id(long id) {
            _id = id;
            return this;
        }
        
        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            ((EditorEnvironment) element.Environment).TransactionManager.InsertTransaction(new RemoveComponentTransaction(_type, _id));
        }
    }
}