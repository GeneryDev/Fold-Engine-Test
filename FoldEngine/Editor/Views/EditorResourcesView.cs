using System;
using FoldEngine.Commands;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Views {
    public class EditorResourcesView : EditorView {
        public EditorResourcesView() {
            Icon = new ResourceIdentifier("editor/checkmark");
        }

        public override string Name => "Resources";

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;

            if(ContentPanel.Button("Save All", 14).IsPressed()) {
                Scene.Resources.SaveAll();
                Console.WriteLine("Save resources!");
            }
            
            if(ContentPanel.Button("Save Scene", 14).IsPressed()) {
                // Scene.Core.CommandQueue.Enqueue(new SaveSceneCommand());
                // Scene.SaveAll();
                // Console.WriteLine("Save resources!");
            }

            if(ContentPanel.Button("Invalidate Caches", 14).IsPressed()) Scene.Resources.InvalidateCaches();
            if(ContentPanel.Button("Invoke GC", 14).IsPressed()) GC.Collect(GC.MaxGeneration);
            
        }
    }
}