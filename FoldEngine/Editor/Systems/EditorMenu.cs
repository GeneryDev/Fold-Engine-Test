using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems {
    [GameSystem("fold:editor.menu", ProcessingCycles.Render)]
    public class EditorMenu : GameSystem {
        private bool ModalActive = false;
        private bool ModalVisible = true;
        
        private GuiPanel _panel = new GuiPanel() {
            Bounds = new Rectangle(EditorRendering.SidebarX + EditorRendering.SidebarMargin*2,
                EditorRendering.SidebarMargin,
                EditorRendering.SidebarWidth - EditorRendering.SidebarMargin * 2 * 2,
                720 - EditorRendering.SidebarMargin * 2)
        };

        public override void OnRender(IRenderingUnit renderer) {
            if(!ModalVisible) return;

            IRenderingLayer layer = renderer.ScreenLayer;
            
            _panel.Reset();
            _panel.Label(Owner.Name, 2).TextAlignment(-1).Icon(renderer.Textures["beacon"]);
            _panel.Button("Save");
            _panel.Separator();
            _panel.Button("Entities");
            _panel.Button("Systems");
            _panel.Button("Edit Save Data");
            _panel.Button("Quit");
            _panel.End();

            _panel.Render(renderer, layer);
        }
    }
}