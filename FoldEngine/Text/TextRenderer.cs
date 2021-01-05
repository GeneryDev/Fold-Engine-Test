using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text {
    //Coordinate system where +Y is down and -Y is up
    public class TextRenderer {
        public static TextRenderer Instance { get; private set; } = new TextRenderer();
        
        private Font _font;
        private string _text;
        private int _index;
        private Point _cursor;
        
        private readonly List<GlyphSource> _sources = new List<GlyphSource>();
        private readonly List<List<RenderedTextGlyph>> _lines = new List<List<RenderedTextGlyph>>();
        private List<RenderedTextGlyph> _currentLine;
        private int _linesRendered;

        public TextRenderer() {
            _lines.Add(new List<RenderedTextGlyph>());
            _currentLine = _lines[0];
        }

        public void Start(Font font, string text) {
            _font = font;
            _text = text;
            _index = 0;
            _cursor = Point.Zero;
            _sources.Clear();
            _currentLine = _lines[0];
            _currentLine.Clear();
            _linesRendered = 0;

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
        }

        public void Render(Font font, string text, out RenderedText output) {
            Start(font, text);
            CreateResult(out output);
        }

        private void CreateResult(out RenderedText output) {
            output = default;

            output.GlyphSources = _sources.ToArray();
            output.Lines = new RenderedTextLine[_linesRendered];
            for(int i = 0; i < _linesRendered; i++) {
                output.Lines[i] = new RenderedTextLine() {
                    Glyphs = _lines[i].ToArray()
                };
            }
        }

        private void FlushLine() {
            if(_currentLine.Count > 0) {
                _linesRendered++;

                if(_linesRendered >= _lines.Count) {
                    _lines.Add(new List<RenderedTextGlyph>());
                }

                _currentLine = _lines[_linesRendered];
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
        
        public void DrawOnto(RenderSurface surface, Point start, Color color, float size = 1) {
            for(int i = 0; i < _linesRendered; i++) {
                foreach(var glyph in _lines[i]) {
                    glyph.DrawOnto(surface, start, color, size, _sources);
                }
            }
        }
    }
}