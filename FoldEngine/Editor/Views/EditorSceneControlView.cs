using System;
using FoldEngine.Commands;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Views {
    public class EditorSceneControlView : EditorView {
        public EditorSceneControlView() {
            Icon = new ResourceIdentifier("editor/info");
        }

        public override string Name => "Scene Controls";

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;

            if(ContentPanel.Button("Save Scene", 14).IsPressed()) {
                Scene.Core.CommandQueue.Enqueue(new SaveSceneCommand(Scene.Identifier ?? "__new_scene"));
            }

            if(ContentPanel.Button("Detach Editor (no undo!)", 14).IsPressed()) {
                SceneEditor.DetachEditor(Scene);
                Console.WriteLine("Editor detached!");
            }

            if(ContentPanel.Button("Breakpoint", 14).IsPressed()) {
                Console.WriteLine("Breakpoint!");
            }
        }
    }
}