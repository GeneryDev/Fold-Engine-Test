﻿using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;

namespace FoldEngine.Editor.Systems {
    [GameSystem("fold:editor.systems", ProcessingCycles.All)]
    public class EditorSystemsList : EditorModal {
        
        private GuiPanel _panel;

        public override void OnRender(IRenderingUnit renderer) {
            if(!ModalVisible) return;
            if(_panel == null) _panel = NewSidebarPanel();

            IRenderingLayer layer = renderer.WindowLayer;
            
            _panel.Reset();
            _panel.Label("Systems", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            _panel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            _panel.Button("Add System");
            _panel.Separator();

            foreach(GameSystem sys in Owner.Systems.AllSystems) {
                _panel.Button(sys.SystemName).TextAlignment(-1);
            }
            
            _panel.End();

            _panel.Render(renderer, layer);
        }
    }
}