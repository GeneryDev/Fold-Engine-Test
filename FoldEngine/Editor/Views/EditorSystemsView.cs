using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorSystemsView : EditorView {
        public override string Icon => "editor:cog";
        public override string Name => "Systems";
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            
            ContentPanel.Button("Add System", 14);
            ContentPanel.Separator();
            
            foreach(GameSystem sys in Scene.Systems.AllSystems) {
                ContentPanel.Button(sys.SystemName, 14).TextAlignment(-1).LeftAction<ViewSystemAction>().System(sys);
            }
        }
    }
    

    public class ViewSystemAction : IGuiAction {
        private GameSystem _system;

        public ViewSystemAction System(GameSystem system) {
            _system = system;
            return this;
        }
        
        public void Perform(GuiElement element, MouseEvent e) {
            if(element.Environment is EditorEnvironment editorEnvironment) {
                editorEnvironment.GetView<EditorInspectorView>().SetObject(_system);
                editorEnvironment.SwitchToView(editorEnvironment.GetView<EditorInspectorView>());
            }
        }

        public IObjectPool Pool { get; set; }
    }
}