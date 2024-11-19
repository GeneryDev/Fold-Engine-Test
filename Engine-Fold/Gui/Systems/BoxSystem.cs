using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Systems;

public partial class ControlRenderer
{
    private void RenderBox(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform, ref Control control, ref BoxControl box)
    {
        var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
        
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = box.Color,
            DestinationRectangle = bounds,
            Z = -control.ZOrder
        });
    }
}