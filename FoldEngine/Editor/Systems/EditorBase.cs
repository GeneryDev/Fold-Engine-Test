using System;
using FoldEngine.Commands;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Woofer;

namespace FoldEngine.Editor.Views {

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
            Environment = new EditorEnvironment();

            Environment.AddView<EditorMenuView>(Owner, Environment.NorthPanel);
            Environment.AddView<EditorHierarchyView>(Owner, Environment.WestPanel);
            Environment.AddView<EditorSystemsView>(Owner, Environment.WestPanel);
            Environment.AddView<EditorInspectorView>(Owner, Environment.EastPanel);
        }

        public override void OnInput() {
            Environment.Input(Owner.Core.InputUnit);

            if(Environment.MouseRight.Down) {
                Environment.SizeEast = (Environment.Layer?.LayerSize.X ?? 1) - Environment.MousePos.X;
                // Owner.Core.RenderingUnit.RootGroup.Dependencies[0].Destination.Location = Mouse.GetState().Position;
            }
        }

        public override void OnUpdate() {
            Environment.Update();
        }

        public override void OnRender(IRenderingUnit renderer) {
            Environment.Render(renderer, renderer.RootGroup["editor_gui"]);
        }
    }
}