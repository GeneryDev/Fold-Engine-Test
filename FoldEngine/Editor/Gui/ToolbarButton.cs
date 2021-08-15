using System;
using FoldEngine.Gui;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui {
    public class ToolbarButton : GuiButton {
        private RenderedText _renderedName;

        private bool _down = false;

        protected override Color NormalColor => _down ? base.PressedColor : base.NormalColor;

        public override void Reset(GuiPanel parent) {
            _renderedName = default;
            _down = false;
            base.Reset(parent);
        }

        public ToolbarButton Down(bool down) {
            _down = down;
            return this;
        }
        
        public override void AdjustSpacing(GuiPanel parent) {
            _renderedName = Parent.RenderString(_text, _fontSize);
            Bounds.Width = (_text.Length > 0 ? Margin*2 + (int)_renderedName.Width : 0) + 16 + _iconSize.X;
            Bounds.Height = 12 * _fontSize / 7;
            Margin = 4;
        }

        public override void Displace(ref Point layoutPosition) {
            layoutPosition.X += Bounds.Width + Margin;
        }
    }
}