using System;
using FoldEngine.Commands;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Woofer;

namespace FoldEngine.Editor {

    [GameSystem("fold:editor.base", ProcessingCycles.All)]
    public class EditorBase : GameSystem {
        // public override bool ShouldSave => false;
        
        public EditorEnvironment Environment;
        
        public override void SubscribeToEvents() {
            Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => {
                Environment.LayoutValidated = false;
            });
        }

        internal override void Initialize() {
            Environment = new EditorEnvironment(Owner);

            Environment.AddView<EditorMenuView>(Environment.NorthPanel);
            Environment.AddView<EditorHierarchyView>(Environment.WestPanel);
            Environment.AddView<EditorSystemsView>(Environment.WestPanel);
            Environment.AddView<EditorInspectorView>(Environment.EastPanel);
        }

        public override void OnInput() {
            Environment.Input(Owner.Core.InputUnit);
        }

        public override void OnUpdate() {
            Environment.Update();
        }

        public override void OnRender(IRenderingUnit renderer) {
            Environment.Render(renderer, renderer.RootGroup["editor_gui"]);
        }
    }
}