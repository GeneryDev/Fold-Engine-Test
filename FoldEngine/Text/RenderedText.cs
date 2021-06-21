using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    public struct RenderedText {
        public Font Font;
        public RenderedTextGlyph[] Glyphs;
        public int Width;
        public int Height;

        public bool HasValue => Glyphs != null;

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size = 1) {
            foreach(RenderedTextGlyph glyph in Glyphs) {
                glyph.DrawOnto(surface, start, color, size, Font);
            }
        }
    }

    public struct RenderedTextGlyph {
        public bool HasValue;
        public int SourceIndex;
        public Rectangle Source;
        public Rectangle Destination;

        public RenderedTextGlyph(bool hasValue) : this() {
            HasValue = hasValue;
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size, Font font) {
            ITexture texture = font.TextureSources[SourceIndex];
            Vector2 textureSize = new Vector2(texture.Width, texture.Height);
            (int x, int y) = start;
            surface.Draw(new DrawQuadInstruction(
                texture,
                new Vector2(size*Destination.Left + x, size*Destination.Bottom + y),
                new Vector2(size*Destination.Left + x, size*Destination.Top + y),
                new Vector2(size*Destination.Right + x, size*Destination.Bottom + y),
                new Vector2(size*Destination.Right + x, size*Destination.Top + y),
                new Vector2(Source.Left, Source.Bottom) / textureSize,
                new Vector2(Source.Left, Source.Top) / textureSize,
                new Vector2(Source.Right, Source.Bottom) / textureSize,
                new Vector2(Source.Right, Source.Top) / textureSize,
                color
            ));
        }
    }
}