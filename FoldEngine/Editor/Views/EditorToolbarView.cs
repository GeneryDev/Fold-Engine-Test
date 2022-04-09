using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EntryProject.Util;
using FoldEngine.Commands;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Tools;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views {
    public class EditorToolbarView : EditorView {
        public override string Name => "Toolbar";

        public EditorToolbarView() {
            Icon = new ResourceIdentifier("editor/cog");
        }

        public override void Render(IRenderingUnit renderer) {
            foreach(EditorTool tool in ((EditorEnvironment) ContentPanel.Environment).Tools) {
                if(ContentPanel.Element<ToolbarButton>()
                    .Down(tool == ((EditorEnvironment) ContentPanel.Environment).ActiveTool)
                    .Text("")
                    .FontSize(14)
                    .Icon(Scene.Resources.Get<Texture>(ref tool.Icon))
                    .IsPressed()) {
                    ((EditorEnvironment) ContentPanel.Environment).SelectedTool = tool;
                }
            }
            
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            // ContentPanel.Element<ToolbarButton>().Text("Save").FontSize(14).Icon(renderer.Textures["editor:cog"]);
            // ContentPanel.Separator();
            if(ContentPanel.Element<ToolbarButton>().Text("").FontSize(14).Icon(Scene.Resources.Get<Texture>(ref EditorIcons.Play)).IsPressed()) {
                if(_storedScene == null) {
                    Play(ContentPanel.Environment as EditorEnvironment);
                } else {
                    Stop(ContentPanel.Environment as EditorEnvironment);
                    Scene.Core.AudioUnit.StopAll();
                    GC.Collect(GC.MaxGeneration);
                }
            }

            if(ContentPanel.Element<ToolbarButton>()
                .Down(Scene.Paused)
                .Text("")
                .FontSize(14)
                .Icon(Scene.Resources.Get<Texture>(ref EditorIcons.Pause))
                .IsPressed()) {
                Scene.Paused = !Scene.Paused;
            }
            // ContentPanel.Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
            // ContentPanel.Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
            // ContentPanel.Button("Edit Save Data").Action(SceneEditor.Actions.Test, 0);
            // ContentPanel.Element<ToolbarButton>().Text("Quit").FontSize(14);
        }

        private static byte[] _storedScene = null;

        private void Play(EditorEnvironment environment) {
            environment.Scene.EditorComponents = null;
            
            var stream = new MemoryStream();
                
            var saveOp = new SaveOperation(stream);
            saveOp.Options.Set(SerializeTempResources.Instance, true);
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