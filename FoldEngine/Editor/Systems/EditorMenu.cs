﻿using System;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems {
    [GameSystem("fold:editor.menu", ProcessingCycles.All)]
    public class EditorMenu : EditorModal {
        
        private GuiPanel _panel;

        internal override void Initialize() {
            _panel = NewSidebarPanel();
            Console.WriteLine("Initialized menu");
        }

        public override void OnRender(IRenderingUnit renderer) {
            if(!ModalVisible) return;

            IRenderingLayer layer = renderer.ScreenLayer;
            
            _panel.Reset();
            _panel.Label(Owner.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            _panel.Button("Save");
            _panel.Separator();
            _panel.Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
            _panel.Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
            _panel.Button("Edit Save Data");
            _panel.Button("Quit");
            _panel.End();

            _panel.Render(renderer, layer);
        }
    }
}