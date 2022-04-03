using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Systems;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorSystemsView : EditorView {
        public override string Name => "Systems";

        public EditorSystemsView() {
            Icon = new ResourceIdentifier("editor/cog");
        }
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;
            
            ContentPanel.Button("Add System", 14);
            ContentPanel.Separator();

            var editorEnvironment = (EditorEnvironment) ContentPanel.Environment;
            
            foreach(GameSystem sys in Scene.Systems.AllSystems) {
                if(ContentPanel.Button(sys.SystemName, 14).TextAlignment(-1).IsPressed()) {
                    editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                    editorEnvironment.GetView<EditorInspectorView>().SetObject(sys);
                    editorEnvironment.SwitchToView(editorEnvironment.GetView<EditorInspectorView>());
                }
            }
        }
    }
}