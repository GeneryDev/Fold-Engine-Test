using System;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Systems;

namespace FoldEngine.Editor.Views {
    public class EditorResourcesView : EditorView {
        public override string Icon => "editor:checkmark";
        public override string Name => "Resources";
        
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;

            if(ContentPanel.Button("Save All", 14).IsPressed()) {
                Scene.Resources.SaveAll();
                Console.WriteLine("Save resources!");
            }
            if(ContentPanel.Button("Unload All", 14).IsPressed()) {
                Scene.Resources.UnloadAll();
                Console.WriteLine("Unloaded all resources!");
            }
        }
    }
}