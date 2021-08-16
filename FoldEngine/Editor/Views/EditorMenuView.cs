using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EntryProject.Util;
using FoldEngine.Commands;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
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
            ContentPanel.Element<ToolbarButton>().Text("").FontSize(14).Icon(renderer.Textures["editor:play"]).LeftAction<PlayStopAction>();
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

    public class PlayStopAction : IGuiAction {
        public IObjectPool Pool { get; set; }
        public void Perform(GuiElement element, MouseEvent e) {
            if(_storedScene == null) {
                Play(element.Environment as EditorEnvironment);
            } else {
                Stop(element.Environment as EditorEnvironment);
            }
        }

        private static byte[] _storedScene = null;

        private void Play(EditorEnvironment environment) {
            environment.Scene.EditorComponents = null;
            
            var stream = new MemoryStream();
                
            var saveOp = new SaveOperation(stream);
            environment.Scene.Save(saveOp);
            
            saveOp.Close();
            _storedScene = stream.GetBuffer();
            saveOp.Dispose();
            environment.Scene.Paused = false;
        }

        private void Stop(EditorEnvironment environment) {
            var loadOp = new LoadOperation(new MemoryStream(_storedScene));
            
            loadOp.Options.Set(DeserializeClearScene.Instance, true);
            
            environment.Scene.Load(loadOp);
            
            loadOp.Close();
            loadOp.Dispose();
            _storedScene = null;
            environment.Scene.EditorComponents = new EditorComponents(environment.Scene);
            environment.Scene.Paused = true;
        }
    }
}