using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    //Coordinate system where +Y is down and -Y is up
    public class TextRenderer {
        private readonly List<RenderedTextGlyph> _glyphs = new List<RenderedTextGlyph>();

        private BitmapFont _bitmapFont;
        private Point _cursor;
        private int _index;
        private int _lineStartIndex;
        private Point _maxPoint;
        private Point _minPoint;

        private float _scale;
        private float _size;
        private string _text;
        public static TextRenderer Instance { get; } = new TextRenderer();

        public Point Cursor => _cursor;

        public int Width => _maxPoint.X - _minPoint.X;
        public int Height => _maxPoint.Y - _minPoint.Y;

        public void Start(IFont font, string text, float size) {
            if(font is FontSet fontSet) font = fontSet.PickFontForSize(size);
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
                if(!Append(c)) break;
                _index++;
            }

            if(_index > _lineStartIndex) FlushLine();
        }

        public bool Append(char c) {
            if(c == '\n') {
                //NEWLINE
                FlushLine();
            } else {
                RenderedTextGlyph glyph = NextGlyph(c);
                if(!glyph.HasValue) return false;
                _minPoint = _minPoint.Min(glyph.Destination.Location);
                _maxPoint = _maxPoint.Max(glyph.Destination.Location + glyph.Destination.Size);
                _glyphs.Add(glyph);
            }

            return true;
        }

        public void DumpChars(IEnumerable<char> chars) {
            foreach(char c in chars) Append(c);
        }

        public void Render(BitmapFont bitmapFont, string text, out RenderedText output, float size) {
            Start(bitmapFont, text, size);
            CreateResult(out output);
        }

        private void CreateResult(out RenderedText output) {
            output = default;

            output.BitmapFont = _bitmapFont;
            output.Text = _text;
            output.Size = _size;
            output.Generation = _bitmapFont.Generation;
            output.Glyphs = _glyphs.ToArray();
            output.Width = Width;
            output.Height = Height;
        }

        private void FlushLine() {
            _cursor.X = 0;
            _cursor.Y += _bitmapFont.LineHeight;

            _lineStartIndex = _index;
        }

        private RenderedTextGlyph NextGlyph(char c) {
            GlyphInfo glyphInfo = _bitmapFont[c];
            if(glyphInfo.NotNull) {
                var glyph = new RenderedTextGlyph {
                    HasValue = true,
                    SourceIndex = glyphInfo.SourceIndex,
                    Source = glyphInfo.Source,
                    Destination = new Rectangle(
                        (int) (_cursor.X * _scale),
                        (int) ((_cursor.Y - glyphInfo.Ascent) * _scale),
                        (int) (glyphInfo.Width * _scale),
                        (int) (glyphInfo.Height * _scale)
                    )
                };

                _cursor.X += glyphInfo.Advancement;

                return glyph;
            }

            return default;
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float scale = 1) {
            foreach(RenderedTextGlyph glyph in _glyphs) glyph.DrawOnto(surface, start, color, scale, _bitmapFont);
        }
    }
}