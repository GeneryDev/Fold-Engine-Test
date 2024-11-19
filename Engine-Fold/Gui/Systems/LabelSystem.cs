using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Systems;

public partial class ControlRenderer
{
    private void RenderLabel(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform, ref Control control, ref LabelControl label)
    {
        var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());

        if (label.UpdateRenderedText(renderer))
        {
            control.ComputedMinimumSize = new Vector2(label.RenderedText.Width, label.RenderedText.Height);
            control.RequestLayout = true;
        }
        ref RenderedText renderedText = ref label.RenderedText;
        if (!renderedText.HasValue) return;

        float textWidth = renderedText.Width;

        int totalWidth = (int)textWidth;

        int x;
        switch (label.Alignment)
        {
            case Alignment.Begin:
                x = bounds.X;
                break;
            case Alignment.Center:
                x = bounds.Center.X - totalWidth / 2;
                break;
            case Alignment.End:
                x = bounds.X + bounds.Width - totalWidth;
                break;
            default:
                x = bounds.X;
                break;
        }

        Point offset = Point.Zero;

        renderedText.DrawOnto(layer.Surface, new Point(x, bounds.Center.Y - renderedText.Height / 2 + label.FontSize) + offset,
            label.Color, z: -control.ZOrder);
        // layer.Surface.Draw(new DrawRectInstruction
        // {
        //     Texture = renderer.WhiteTexture,
        //     Color = Color.White,
        //     DestinationRectangle = new Rectangle(new Point(x - 2, bounds.Center.Y - renderedText.Height / 2 + label.FontSize), new Point(2, 2)),
        //     Z = control.ZOrder
        // });
    }
}