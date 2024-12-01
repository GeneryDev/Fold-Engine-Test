using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components.Controls
{
    [Component("fold:control.box", traits: [typeof(Control), typeof(MouseFilterDefaultStop)])]
    public struct BoxControl
    {
        public Color Color;
    }
}

namespace FoldEngine.Gui.Systems
{
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
}