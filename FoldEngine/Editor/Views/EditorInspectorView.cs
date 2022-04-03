using System;
using System.Globalization;
using System.Runtime.CompilerServices;
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

        private object _object = null;

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            long id = -1;
            var editorBase = Scene.Systems.Get<EditorBase>();
            if(editorBase.EditingEntity.Count == 1) id = editorBase.EditingEntity[0];
            
            if(id != -1 && Scene.Components.HasComponent<Transform>(id)) {
                RenderEntityView(renderer, id);
            } else if(_object != null) {
                RenderObjectView(renderer);
            }
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        }

        private void RenderEntityView(IRenderingUnit renderer, long id) {
            ContentPanel.Label(Scene.Components.GetComponent<EntityName>(id).Name, 14)
                .TextAlignment(-1)
                .Icon(renderer.Textures["editor:cube"]);
            ContentPanel.Label($"ID: {id}", 7).TextAlignment(-1);
            ContentPanel.Spacing(12);

            foreach(ComponentSet set in Scene.Components.Sets.Values) {
                // if(set.ComponentType == typeof(EntityName)) continue;
                if(set.Has((int) id)) {
                    ComponentInfo componentInfo = ComponentInfo.Get(set.ComponentType);
                    if(componentInfo.HideInInspector) continue;

                    ContentPanel.Spacing(5);

                    ContentPanel.Element<ComponentHeader>()
                        .Info(componentInfo)
                        .Id(id);

                    // ContentPanel.Label(componentInfo.Name, 14).TextAlignment(-1);

                    foreach(ComponentMember member in componentInfo.Members) {
                        if(!member.ShouldShowInInspector(Scene, id)) continue;
                        object value = set.GetFieldValue((int) id, member.FieldInfo);
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

                        member.ForEntity(set, id).CreateInspectorElement(ContentPanel, value);

                        ContentPanel.Element<ComponentMemberBreak>();
                        // ContentPanel.Spacing(5);
                    }
                }
            }

            if(ContentPanel.Button("Add Component", 14).IsPressed(out Point p)) {
                GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
                contextMenu.Show(p, m => {
                    foreach(Type type in Component.GetAllTypes()) {
                        if(!Scene.Components.HasComponent(type, id) && m.Button(type.Name, 9).IsPressed()) {
                            ((EditorEnvironment) ContentPanel.Environment).TransactionManager.InsertTransaction(new AddComponentTransaction(type, id));
                        }
                    }
                });
            }
        }

        private void RenderObjectView(IRenderingUnit renderer) {
            ComponentInfo info = ComponentInfo.Get(_object.GetType());
            
            ContentPanel.Label(info.Name, 14)
                .TextAlignment(-1)
                .Icon(renderer.Textures["editor:cog"]);
            ContentPanel.Spacing(12);
            

            foreach(ComponentMember member in info.Members) {
                object value = member.FieldInfo.GetValue(_object);

                ContentPanel.Element<ComponentMemberLabel>().Member(member);

                member.ForObject(_object).CreateInspectorElement(ContentPanel, value);

                ContentPanel.Element<ComponentMemberBreak>();
            }
        }

        public void SetObject(object obj) {
            _object = obj;
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

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default) {
        }

        public override void Displace(ref Point layoutPosition) {
            layoutPosition.X = Parent.Bounds.X;
            layoutPosition.Y += 20;
        }
    }

    public class ComponentHeader : GuiLabel {
        private ComponentInfo _info;
        private long _id;

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
                Environment.ContextMenu.Show(e.Position, m => {
                    if(m.Button("Remove", 14).IsPressed()) {
                        ((EditorEnvironment) Environment).TransactionManager.InsertTransaction(new RemoveComponentTransaction(_info.ComponentType, _id));
                    };
                });
            }
            base.OnMouseReleased(ref e);
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default) {
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds.Translate(offset)
            });
            // _textColor = Rollover ? Color.CornflowerBlue : Color.White;
            base.Render(renderer, layer, offset);
        }
    }
}