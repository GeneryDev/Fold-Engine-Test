using System;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorMenuView : EditorView {
        public override string Icon => "editor:cog";
        public override string Name => "Toolbar";

        public override void Render(IRenderingUnit renderer) {
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            // ContentPanel.Element<ToolbarButton>().Text("Save").FontSize(14).Icon(renderer.Textures["editor:cog"]);
            ContentPanel.Element<ToolbarButton>().Text("").FontSize(14).Icon(renderer.Textures["editor:play"]);
            ContentPanel.Element<ToolbarButton>().Down(Scene.Paused).Text("").FontSize(14).Icon(renderer.Textures["editor:pause"]).LeftAction<PauseAction>();
            // ContentPanel.Separator();
            // ContentPanel.Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
            // ContentPanel.Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
            // ContentPanel.Button("Edit Save Data").Action(SceneEditor.Actions.Test, 0);
            // ContentPanel.Element<ToolbarButton>().Text("Quit").FontSize(14);
        }
    }

    public class PauseAction : IGuiAction {
        public IObjectPool Pool { get; set; }
        public void Perform(GuiElement element, MouseEvent e) {
            element.Environment.Scene.Paused = !element.Environment.Scene.Paused;
        }
    }
}