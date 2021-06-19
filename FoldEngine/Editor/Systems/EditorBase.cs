using System;
using FoldEngine.Commands;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Woofer;

namespace FoldEngine.Editor.Systems {

    [GameSystem("fold:editor.base", ProcessingCycles.All)]
    public class EditorBase : GameSystem {
        public const int SidebarX = 1280 * 4 / 5;
        public const int SidebarWidth = 1280 / 5;

        public const int SidebarMargin = 4;

        // public override bool ShouldSave => false;
        
        public EditorEnvironment Environment;
        
        private Type[] _modalTypes = new Type[] {typeof(EditorMenu), typeof(EditorEntitiesList), typeof(EditorSystemsList)};
        private bool ReadjustViewport;

        public override void SubscribeToEvents() {
            Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => {
                Environment.LayoutValidated = false;
            });
        }

        internal override void Initialize() {
            Environment = new EditorEnvironment();
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