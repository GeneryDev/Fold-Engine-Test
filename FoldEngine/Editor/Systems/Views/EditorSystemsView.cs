using FoldEngine.Components;
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
                ContentPanel.Button(sys.SystemName, 14).TextAlignment(-1);
            }
        }
    }
}