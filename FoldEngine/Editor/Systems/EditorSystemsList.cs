using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Woofer;

namespace FoldEngine.Editor.Systems {
    [GameSystem("fold:editor.systems", ProcessingCycles.All)]
    public class EditorSystemsList : EditorModal {
        
        private GuiPanel _panel;

        public override void SubscribeToEvents() {
            base.SubscribeToEvents();
            Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => {
                if(_panel != null) {
                    _panel.Bounds.Height = evt.NewSize.Y;
                    _panel.Bounds.X = evt.NewSize.X - _panel.Bounds.Width - EditorBase.SidebarMargin * 2;
                }
            });
        }

        public override void OnRender(IRenderingUnit renderer) {
            if(!ModalVisible) return;
            IRenderingLayer layer = renderer.RootGroup["editor_gui"];
            if(_panel == null) _panel = NewSidebarPanel(layer);

            _panel.Reset();
            _panel.Label("Systems", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            _panel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            _panel.Button("Add System");
            _panel.Separator();

            foreach(GameSystem sys in Owner.Systems.AllSystems) {
                _panel.Button(sys.SystemName).TextAlignment(-1);
            }
            
            _panel.End();

            _panel.Render(renderer, layer);
        }
    }
}