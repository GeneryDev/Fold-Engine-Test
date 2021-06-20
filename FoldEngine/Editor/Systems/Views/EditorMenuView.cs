using System;
using FoldEngine.Editor.Views;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorMenuView : EditorView {
        public override string Icon => "editor:cog";
        public override string Name => "Menu?";

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            ContentPanel.Button("Save");
            ContentPanel.Separator();
            ContentPanel.Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
            ContentPanel.Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
            ContentPanel.Button("Edit Save Data").Action(SceneEditor.Actions.Test, 0);
            ContentPanel.Button("Quit");
        }
    }
}