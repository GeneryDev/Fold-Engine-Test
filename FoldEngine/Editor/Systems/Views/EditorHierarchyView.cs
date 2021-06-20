using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Sandbox.Components;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorHierarchyView : EditorView {
        
        private ComponentIterator<Transform> _transforms;

        private List<long> _expandedEntities = new List<long>();

        public override string Icon => "editor:cog";
        public override string Name => "Hierarchy";

        public override void Initialize() {
            _transforms = Scene.Components.CreateIterator<Transform>(IterationFlags.None);
        }

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
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
            
            panel.Button(Scene.Components.GetComponent<EntityName>(entityId).Name)
                .TextAlignment(-1)
                .Icon(renderer.Textures[hasChildren ? expanded ? "editor:triangle.down" : "editor:triangle.right" : "editor:blank"])
                .Action(SceneEditor.Actions.ExpandCollapseEntity, entityId)
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
}