using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {

    public class BitmapFont : IFont {
        public readonly List<string> TextureNames = new List<string>();
        public readonly List<ITexture> TextureSources = new List<ITexture>();
        public readonly GlyphBlock[] GlyphBlocks = new GlyphBlock[256];

        public int LineHeight = 10;
        public float DefaultSize = 9;
        
        public GlyphInfo this[char c] {
            get => GlyphBlocks[c / 256]?.Glyphs[c % 256] ?? default;
            set {
                if(GlyphBlocks[c / 256] == null) GlyphBlocks[c / 256] = new GlyphBlock(c / 256);
                GlyphBlocks[c / 256].Glyphs[c % 256] = value;
            }
        }

        public void RenderString(string text, out RenderedText rendered, float size) {
            TextRenderer.Instance.Render(this, text, out rendered, size);
        }

        public void DrawString(string text, RenderSurface surface, Point start, Color color, float size) {
            TextRenderer.Instance.Start(this, text, size);
            TextRenderer.Instance.DrawOnto(surface, start, color);
        }
    }

    public class GlyphBlock {
        public readonly int BlockStart;
        public readonly GlyphInfo[] Glyphs;
        
        public GlyphBlock(int blockStart) {
            BlockStart = blockStart;
            Glyphs = new GlyphInfo[256];
        }
    }

    public struct GlyphInfo {
        public bool NotNull;
        public int SourceIndex;
        public Rectangle Source;
        public int Height;
        public int Ascent;
        public int Width;
        public int Advancement;

        public override string ToString() {
            return $"{nameof(NotNull)}: {NotNull}, {nameof(SourceIndex)}: {SourceIndex}, {nameof(Source)}: {Source}, {nameof(Height)}: {Height}, {nameof(Ascent)}: {Ascent}, {nameof(Width)}: {Width}, {nameof(Advancement)}: {Advancement}";
        }
    }
}