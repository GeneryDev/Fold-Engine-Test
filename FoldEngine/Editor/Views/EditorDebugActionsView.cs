using System;
using FoldEngine.Commands;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Views {
    public class EditorDebugActionsView : EditorView {
        public EditorDebugActionsView() {
            Icon = new ResourceIdentifier("editor/info");
        }

        public override string Name => "Debug Actions";

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
            
            
            if(ContentPanel.Button("Save All Resources", 14).IsPressed()) {
                Scene.Resources.SaveAll();
                Console.WriteLine("Save resources!");
            }

            if(ContentPanel.Button("Reload Resources", 14).IsPressed()) Scene.Core.ResourceIndex.Update();
            if(ContentPanel.Button("Invoke GC", 14).IsPressed()) GC.Collect(GC.MaxGeneration);
        }
    }
}