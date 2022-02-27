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
            
            ContentPanel.Button("Save All", 14).LeftAction<SaveResourcesAction>();
        }
        
        private class SaveResourcesAction : IGuiAction {
        
            public void Perform(GuiElement element, MouseEvent e) {
                element.Environment.Scene.Resources.SaveAll();
                Console.WriteLine("Save resources!");
            }

            public IObjectPool Pool { get; set; }
        }
    }
}