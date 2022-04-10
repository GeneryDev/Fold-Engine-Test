using System;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Transactions;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views {
    public class EditorSystemsView : EditorView {
        public EditorSystemsView() {
            Icon = new ResourceIdentifier("editor/cog");
        }

        public override string Name => "Systems";

        public override void Render(IRenderingUnit renderer) {
            ContentPanel.MayScroll = true;

            if(ContentPanel.Button("Add System", 14).IsPressed(out Point p)) {
                GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
                contextMenu.Show(p, m => {
                    foreach(Type type in GameSystem.GetAllTypes())
                        if(Scene.Systems.Get(type) == null && m.Button(type.Name, 9).IsPressed())
                            ((EditorEnvironment) ContentPanel.Environment).TransactionManager.InsertTransaction(
                                new AddSystemTransaction(type));
                });
            }
            ContentPanel.Separator();

            var editorEnvironment = (EditorEnvironment) ContentPanel.Environment;

            foreach(GameSystem sys in Scene.Systems.AllSystems) {
                var button = ContentPanel.Button(sys.SystemName, 14).TextAlignment(-1);
                if(button.IsPressed()) {
                    editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                    editorEnvironment.GetView<EditorInspectorView>().SetObject(sys);
                    editorEnvironment.SwitchToView(editorEnvironment.GetView<EditorInspectorView>());
                } else if(button.IsPressed(out Point p2, MouseEvent.RightButton)) {
                    GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
                    contextMenu.Show(p2, m => {
                        if(m.Button("Remove", 14).IsPressed())
                            ((EditorEnvironment) ContentPanel.Environment).TransactionManager.InsertTransaction(
                                new RemoveSystemTransaction(sys.GetType()));
                    });
                }
            }
        }
    }
}