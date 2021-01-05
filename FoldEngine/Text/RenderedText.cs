﻿using System;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    public struct RenderedText {
        public GlyphSource[] GlyphSources;
        public RenderedTextLine[] Lines;

        public float Width {
            get {
                float width = 0;
                foreach(RenderedTextLine line in Lines) {
                    width = Math.Max(width, line.Width);
                }

                return width;
            }
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size = 1) {
            foreach(var line in Lines) {
                line.DrawOnto(surface, start, color, size, GlyphSources);
            }
        }
    }

    public struct RenderedTextLine {
        public RenderedTextGlyph[] Glyphs;

        public float Width {
            get {
                float minX = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;

                foreach(RenderedTextGlyph glyph in Glyphs) {
                    minX = Math.Min(minX, glyph.Destination.Left);
                    maxX = Math.Max(maxX, glyph.Destination.Right);
                }

                if(minX.Equals(float.PositiveInfinity)) return 0;

                return maxX - minX;
            }
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size, GlyphSource[] glyphSources) {
            foreach(RenderedTextGlyph glyph in Glyphs) {
                glyph.DrawOnto(surface, start, color, size, glyphSources);
            }
        }
    }

    public struct RenderedTextGlyph {
        public int SourceIndex;
        public Rectangle Source;
        public Rectangle Destination;
        
        public void DrawOnto(RenderSurface surface, Point start, Color color, float size, GlyphSource[] glyphSources) {
            ITexture texture = glyphSources[SourceIndex].Texture;
            Vector2 textureSize = new Vector2(texture.Width, texture.Height);
            surface.Draw(new DrawQuadInstruction(
                texture,
                new Vector2(size*Destination.Left + start.X, size*Destination.Bottom + start.Y),
                new Vector2(size*Destination.Left + start.X, size*Destination.Top + start.Y),
                new Vector2(size*Destination.Right + start.X, size*Destination.Bottom + start.Y),
                new Vector2(size*Destination.Right + start.X, size*Destination.Top + start.Y),
                new Vector2(Source.Left, Source.Bottom) / textureSize,
                new Vector2(Source.Left, Source.Top) / textureSize,
                new Vector2(Source.Right, Source.Bottom) / textureSize,
                new Vector2(Source.Right, Source.Top) / textureSize,
                color,
                color,
                color,
                color
            ));
        }
    }

    public struct GlyphSource {
        public string Name;
        public ITexture Texture;
    }
}