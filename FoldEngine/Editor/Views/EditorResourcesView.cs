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

            if(ContentPanel.Button("Reload Resources", 14).IsPressed()) Scene.Core.ResourceIndex.Update();
            if(ContentPanel.Button("Invoke GC", 14).IsPressed()) GC.Collect(GC.MaxGeneration);
        }
    }
}