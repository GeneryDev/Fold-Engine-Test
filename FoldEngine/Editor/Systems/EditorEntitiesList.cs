using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Sandbox.Components;

namespace FoldEngine.Editor.Systems {
    [GameSystem("fold:editor.entities", ProcessingCycles.All)]
    public class EditorEntitiesList : EditorModal {
        
        private GuiPanel _panel;
        private ComponentIterator<Transform> _transforms;

        private List<long> _expandedEntities = new List<long>();
        
        internal override void Initialize() {
            _panel = NewSidebarPanel();
            _transforms = Owner.Components.CreateIterator<Transform>(IterationFlags.None);
        }
        

        public override void OnRender(IRenderingUnit renderer) {
            if(!ModalVisible) return;

            IRenderingLayer layer = renderer.ScreenLayer;
            
            _panel.Reset();
            _panel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            _panel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            _panel.Button("New Entity");
            _panel.Separator();
            
            _transforms.Reset();
            while(_transforms.Next()) {
                ref Transform current = ref _transforms.GetComponent();
                if(!current.Parent.IsNotNull) {
                    RenderEntity(ref current, renderer, layer);
                }
            }
            
            _panel.End();

            _panel.Render(renderer, layer);
        }

        private void RenderEntity(ref Transform transform, IRenderingUnit renderer, IRenderingLayer layer, int depth = 0) {
            long entityId = transform.EntityId;
            
            bool hasChildren = transform.FirstChildId != -1;
            bool expanded = _expandedEntities.Contains(entityId);
            
            _panel.Button(Owner.Components.GetComponent<EntityName>(entityId).Name)
                .TextAlignment(-1)
                .Icon(renderer.Textures[hasChildren ? expanded ? "editor:triangle.down" : "editor:triangle.right" : "editor:blank"])
                .Action(SceneEditor.Actions.ExpandCollapseEntity, entityId)
                ;

            if(hasChildren && expanded) {
                foreach(ComponentReference<Transform> childTransform in transform.Children) {
                    RenderEntity(ref childTransform.Get(), renderer, layer, depth + 1);
                }
            }
        }

        public void ExpandCollapseEntity(long entityId) {
            if(!_expandedEntities.Remove(entityId)) _expandedEntities.Add(entityId);
        }
    }
}