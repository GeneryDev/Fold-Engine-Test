using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Sandbox.Components;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorHierarchyView : EditorView {
        
        private ComponentIterator<Transform> _transforms;

        private List<long> _expandedEntities = new List<long>();

        public override string Icon => "editor:hierarchy";
        public override string Name => "Hierarchy";

        public override void Initialize() {
            _transforms = Scene.Components.CreateIterator<Transform>(IterationFlags.None);
        }

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            
            // ContentPanel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            // ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            ContentPanel.Button("New Entity", 14);
            ContentPanel.Separator();
            
            _transforms.Reset();
            while(_transforms.Next()) {
                ref Transform current = ref _transforms.GetComponent();
                if(!current.Parent.IsNotNull) {
                    RenderEntity(ref current, ContentPanel, renderer);
                }
            }
        }

        private void RenderEntity(ref Transform transform, GuiPanel panel, IRenderingUnit renderer, int depth = 0) {
            long entityId = transform.EntityId;
            
            bool hasChildren = transform.FirstChildId != -1;
            bool expanded = _expandedEntities.Contains(entityId);

            var button = panel.Button(Scene.Components.GetComponent<EntityName>(entityId).Name, 14)
                .TextAlignment(-1)
                .Icon(renderer.Textures[
                    hasChildren ? expanded ? "editor:triangle.down" : "editor:triangle.right" : "editor:blank"])
                ;
            
            button.LeftAction<HierarchyAction>()
                .Id(entityId)
                .Depth(depth)
                ;
            
            button.RightAction<ShowEntityContextMenu>()
                .Id(entityId)
                ;

            if(hasChildren && expanded) {
                foreach(ComponentReference<Transform> childTransform in transform.Children) {
                    RenderEntity(ref childTransform.Get(), panel, renderer, depth + 1);
                }
            }
        }

        public void ExpandCollapseEntity(long entityId) {
            if(!_expandedEntities.Remove(entityId)) _expandedEntities.Add(entityId);
        }
    }

    public class HierarchyAction : IGuiAction {
        private long _id;
        private int _depth;

        public HierarchyAction Id(long id) {
            _id = id;
            return this;
        }
        
        public HierarchyAction Depth(int depth) {
            _depth = depth;
            return this;
        }

        public void Perform(GuiElement element, MouseEvent e) {
            if(element.Parent.Environment is EditorEnvironment editorEnvironment) {
                if(e.Position.X < element.Bounds.X + 24) {
                    editorEnvironment.GetView<EditorHierarchyView>().ExpandCollapseEntity(_id);
                } else {
                    editorEnvironment.GetView<EditorInspectorView>().SetEntity(_id);
                }
            }
        }

        public IObjectPool Pool { get; set; }
    }

    public class ShowEntityContextMenu : IGuiAction {
        private long _id;
        
        public ShowEntityContextMenu Id(long id) {
            _id = id;
            return this;
        }

        public void Perform(GuiElement element, MouseEvent e) {
            var contextMenu = element.Parent.Environment.ContextMenu;
            contextMenu.Reset(e.Position);
            contextMenu.Button("Edit", 14).LeftAction<DebugAction>();
            contextMenu.Button("Rename", 14).LeftAction<DebugAction>();
            contextMenu.Button("Delete", 14).LeftAction<DebugAction>();
            contextMenu.Show();
        }
        
        public IObjectPool Pool { get; set; }
    }

    public class DebugAction : IGuiAction {
        public IObjectPool Pool { get; set; }
        public void Perform(GuiElement element, MouseEvent e) {
            Console.WriteLine("debug action");
        }
    }
}