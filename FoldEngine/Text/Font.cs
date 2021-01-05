using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    public class Font {
        public readonly Dictionary<string, ITexture> Textures = new Dictionary<string, ITexture>();

        public int LineHeight => 10;
        
        public void RenderString(string text, out RenderedText rendered) {
            TextRenderer.Instance.Render(this, text, out rendered);
        }

        public void DrawString(string text, RenderSurface surface, Point start, Color color, float size = 1) {
            TextRenderer.Instance.Start(this, text);
            TextRenderer.Instance.DrawOnto(surface, start, color, size);
        }
    }

    public struct GlyphInfo {
        public int SourceIndex;
        public Rectangle Source;
        public float Height;
        public float Ascent;
        public float Width;
    }
}