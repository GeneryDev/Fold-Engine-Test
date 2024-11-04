using FoldEngine.Gui;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui;

public class ToolbarButton : GuiButton
{
    private bool _down;
    private RenderedText _renderedName;

    protected override Color NormalColor => _down ? base.PressedColor : base.NormalColor;

    public override void Reset(GuiPanel parent)
    {
        _renderedName = default;
        _down = false;
        base.Reset(parent);
    }

    public ToolbarButton Down(bool down)
    {
        _down = down;
        return this;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        _renderedName = Parent.RenderString(_text, _fontSize);
        Bounds.Width = 12 * _fontSize / 7;
        Bounds.Height = 12 * _fontSize / 7;
        Margin = 4;
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition.X += Bounds.Width + Margin;
    }
}