using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    public class Font {
        public readonly Dictionary<string, ITexture> Textures = new Dictionary<string, ITexture>();

        public int LineHeight => 10;
        
        public void RenderString(string text, out RenderedText rendered) {
            FontRenderer.Instance.Render(this, text, out rendered);
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