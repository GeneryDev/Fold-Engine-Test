using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    public interface IFont {
        void RenderString(string text, out RenderedText renderedText, float size);
        void DrawString(string text, RenderSurface surface, Point start, Color color, float size);
    }
}