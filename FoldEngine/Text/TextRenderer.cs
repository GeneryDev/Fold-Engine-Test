using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    //Coordinate system where +Y is down and -Y is up
    public class TextRenderer {
        public static TextRenderer Instance { get; } = new TextRenderer();
        
        private BitmapFont _bitmapFont;
        private string _text;
        private float _size;
        private int _index;
        private Point _cursor;
        private Point _minPoint;
        private Point _maxPoint;
        
        private readonly List<RenderedTextGlyph> _glyphs = new List<RenderedTextGlyph>();
        private int _lineStartIndex = 0;

        public int Width => _maxPoint.X - _minPoint.X;
        public int Height => _maxPoint.Y - _minPoint.Y;

        private float _scale;

        public void Start(IFont font, string text, float size) {
            if(font is FontSet fontSet) {
                font = fontSet.PickFontForSize(size);
            }
            _bitmapFont = (BitmapFont) font;
            _text = text;
            _size = size;
            _scale = _size / _bitmapFont.DefaultSize;
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

        public void Render(BitmapFont bitmapFont, string text, out RenderedText output, float size) {
            Start(bitmapFont, text, size);
            CreateResult(out output);
        }

        private void CreateResult(out RenderedText output) {
            output = default;

            output.BitmapFont = _bitmapFont;
            output.Glyphs = _glyphs.ToArray();
            output.Width = Width;
            output.Height = Height;
        }

        private void FlushLine() {
            if(_index > _lineStartIndex) {
                _cursor.X = 0;
                _cursor.Y += _bitmapFont.LineHeight;
                
                _lineStartIndex = _index;
            }
        }
        
        private RenderedTextGlyph NextGlyph() {
            char c = _text[_index];
            GlyphInfo glyphInfo = _bitmapFont[c];
            if(glyphInfo.NotNull) {
                
                var glyph = new RenderedTextGlyph {
                    HasValue = true,
                    SourceIndex = glyphInfo.SourceIndex,
                    Source = glyphInfo.Source,
                    Destination = new Rectangle(
                        (int) (_cursor.X * _scale), 
                        (int) ((_cursor.Y - glyphInfo.Ascent) * _scale),
                        (int) (glyphInfo.Width*_scale),
                        (int) (glyphInfo.Height*_scale)
                    )
                };

                _cursor.X += glyphInfo.Advancement;
                _index++;
                return glyph;
            }

            return default;
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float scale = 1) {
            foreach(RenderedTextGlyph glyph in _glyphs) {
                glyph.DrawOnto(surface, start, color, scale, _bitmapFont);
            }
        }
    }
}