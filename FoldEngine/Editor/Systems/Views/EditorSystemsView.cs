using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorSystemsView : EditorView {
        public override string Icon => "editor:cog";
        public override string Name => "Systems";
        public override void Render(IRenderingUnit renderer) {
            ContentPanel.Label("Systems", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            ContentPanel.Button("Add System");
            ContentPanel.Separator();
            
            foreach(GameSystem sys in Scene.Systems.AllSystems) {
                ContentPanel.Button(sys.SystemName).TextAlignment(-1);
            }
        }
    }
}