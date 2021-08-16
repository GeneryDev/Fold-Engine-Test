using System;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views {
    public class EditorGameView : EditorView {
        public override string Icon => "editor:play";
        public override string Name => "Game";

        public override bool UseMargin => false;
        public override Color? BackgroundColor => Color.Black;

        public override void Render(IRenderingUnit renderer) {
            renderer.Groups["editor"].Dependencies[0].Group.Size = ContentPanel.Bounds.Size;
            renderer.Groups["editor"].Dependencies[0].Destination = ContentPanel.Bounds;
        }

        public override void EnsurePanelExists(GuiEnvironment environment) {
            if(ContentPanel == null) {
                ContentPanel = new GameViewPanel(this, environment);
            }
        }
    }

    public class GameViewPanel : GuiPanel {
        public GameViewPanel(EditorGameView editorGameView, GuiEnvironment environment) : base(environment) {
            MayScroll = true;
        }

        public override void Scroll(int dir) {
            Environment.Scene.EditorComponents.EditorTransform.LocalScale -=
                Environment.Scene.EditorComponents.EditorTransform.LocalScale * 0.1f * dir;
        }
    }
}