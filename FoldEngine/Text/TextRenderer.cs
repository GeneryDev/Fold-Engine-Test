using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    //Coordinate system where +Y is down and -Y is up
    public class TextRenderer {
        public static TextRenderer Instance { get; } = new TextRenderer();
        
        private Font _font;
        private string _text;
        private int _index;
        private Point _cursor;
        private Point _minPoint;
        private Point _maxPoint;
        
        private readonly List<RenderedTextGlyph> _glyphs = new List<RenderedTextGlyph>();
        private int _lineStartIndex = 0;

        public int Width => _maxPoint.X - _minPoint.X;
        public int Height => _maxPoint.Y - _minPoint.Y;

        public void Start(Font font, string text) {
            _font = font;
            _text = text;
            _index = 0;
            _cursor = Point.Zero;
            _glyphs.Clear();
            _lineStartIndex = 0;
            _minPoint = _maxPoint = Point.Zero;

            while(_index < _text.Length) {
                char c = _text[_index];
                if(c == '\n') {
                    //NEWLINE
                    FlushLine();
                    _index++;
                } else {
                    RenderedTextGlyph glyph = NextGlyph();
                    if(!glyph.HasValue) break;
                    _minPoint = _minPoint.Min(glyph.Destination.Location);
                    _maxPoint = _maxPoint.Max(glyph.Destination.Location + glyph.Destination.Size);
                    _glyphs.Add(glyph);
                }
            }
            FlushLine();
        }

        public void Render(Font font, string text, out RenderedText output) {
            Start(font, text);
            CreateResult(out output);
        }

        private void CreateResult(out RenderedText output) {
            output = default;

            output.Font = _font;
            output.Glyphs = _glyphs.ToArray();
            output.Width = Width;
            output.Height = Height;
        }

        private void FlushLine() {
            if(_index > _lineStartIndex) {
                _cursor.X = 0;
                _cursor.Y += _font.LineHeight;
                
                _lineStartIndex = _index;
            }
        }
        
        private RenderedTextGlyph NextGlyph() {
            char c = _text[_index];
            GlyphInfo glyphInfo = _font[c];
            if(glyphInfo.NotNull) {
                
                var glyph = new RenderedTextGlyph {
                    HasValue = true,
                    SourceIndex = glyphInfo.SourceIndex,
                    Source = glyphInfo.Source,
                    Destination = new Rectangle(
                        _cursor.X, 
                        _cursor.Y - glyphInfo.Ascent,
                        glyphInfo.Width,
                        glyphInfo.Height
                    )
                };

                _cursor.X += glyphInfo.Advancement;
                _index++;
                return glyph;
            }

            return default;
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size = 1) {
            foreach(RenderedTextGlyph glyph in _glyphs) {
                glyph.DrawOnto(surface, start, color, size, _font);
            }
        }
    }
}