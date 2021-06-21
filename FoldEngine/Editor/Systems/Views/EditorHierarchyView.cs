using System;
using System.Collections.Generic;
using FoldEngine.Components;
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
            ContentPanel.Button("New Entity");
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
            
            panel.Element<HierarchyButton>()
                .Id(entityId)
                .Depth(depth)
                .Text(Scene.Components.GetComponent<EntityName>(entityId).Name)
                .TextAlignment(-1)
                .Icon(renderer.Textures[hasChildren ? expanded ? "editor:triangle.down" : "editor:triangle.right" : "editor:blank"])
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

    public class HierarchyButton : GuiButton {
        private long _id;
        private int _depth;

        public HierarchyButton Id(long id) {
            _id = id;
            return this;
        }
        
        public HierarchyButton Depth(int depth) {
            _depth = depth;
            return this;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Bounds.X += 12 * _depth;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            base.Render(renderer, layer);
        }

        public override void PerformAction(Point point) {
            if(Parent.Environment is EditorEnvironment editorEnvironment) {
                if(point.X < Bounds.X + 24) {
                    editorEnvironment.GetView<EditorHierarchyView>().ExpandCollapseEntity(_id);
                } else {
                    editorEnvironment.GetView<EditorInspectorView>().SetEntity(_id);
                }
            }
        }
    }
}