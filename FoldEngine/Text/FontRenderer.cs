using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    //Coordinate system where +Y is down and -Y is up
    public class FontRenderer {
        public static FontRenderer Instance { get; private set; } = new FontRenderer();
        
        private Font _font;
        private string _text;
        private int _index;
        private Point _cursor;
        
        private readonly List<GlyphSource> _sources = new List<GlyphSource>();
        private readonly List<RenderedTextGlyph> _currentLine = new List<RenderedTextGlyph>();
        private readonly List<RenderedTextLine> _lines = new List<RenderedTextLine>();

        public void Render(Font font, string text, out RenderedText output) {
            _font = font;
            _text = text;
            _index = 0;
            _cursor = Point.Zero;
            _sources.Clear();
            _currentLine.Clear();
            _lines.Clear();

            while(_index < _text.Length) {
                char c = _text[_index];
                if(c == '\n') {
                    //NEWLINE
                    FlushLine();
                    _index++;
                } else {
                    RenderedTextGlyph? glyph = NextGlyph();
                    if(!glyph.HasValue) break;
                    _currentLine.Add(glyph.Value);
                }
            }
            FlushLine();

            CreateOutput(out output);
        }

        private void CreateOutput(out RenderedText output) {
            output = default;

            output.GlyphSources = _sources.ToArray();
            output.Lines = _lines.ToArray();
        }

        private void FlushLine() {
            if(_currentLine.Count > 0) {
                _lines.Add(new RenderedTextLine() {
                    Glyphs = _currentLine.ToArray()
                });
                _currentLine.Clear();
                _cursor.X = 0;
                _cursor.Y += _font.LineHeight;
            }
        }

        private RenderedTextGlyph? NextGlyph() {
            char c = _text[_index];
            if(c >= 0x00 && c <= 0xFF) { //ASCII

                int height = 8;
                int ascent = 7;
                
                int width = 6;
                int advancement = 7;

                RenderedTextGlyph glyph = new RenderedTextGlyph();
                glyph.SourceIndex = GetSourceIndex("ascii");
                glyph.Source.X = (c & 0xF) * 8;
                glyph.Source.Y = ((c & 0xF0) >> 4) * 8;
                glyph.Source.Width = width;
                glyph.Source.Height = height;

                (glyph.Destination.X, glyph.Destination.Y) = _cursor;
                glyph.Destination.X += advancement - width;
                glyph.Destination.Y -= ascent;
                glyph.Destination.Width = width;
                glyph.Destination.Height = height;

                _cursor.X += advancement;
                _index++;
                return glyph;
            }

            return null;
        }

        private int GetSourceIndex(string key) {
            for(int i = 0; i < _sources.Count; i++) {
                if(_sources[i].Name == key) return i;
            }
            _sources.Add(new GlyphSource() {
                Name = key,
                Texture = _font.Textures[key]
            });
            return _sources.Count - 1;
        }
    }
}