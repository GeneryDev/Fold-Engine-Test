using System;
using System.Collections.Generic;
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
        
        private readonly List<List<RenderedTextGlyph>> _lines = new List<List<RenderedTextGlyph>>();
        private List<RenderedTextGlyph> _currentLine;
        private int _linesRendered;
        
        public float Width {
            get {
                float width = 0;
                foreach(List<RenderedTextGlyph> line in _lines) {
                    float lineWidth = 0;
                    float minX = float.PositiveInfinity;
                    float maxX = float.NegativeInfinity;

                    foreach(RenderedTextGlyph glyph in line) {
                        minX = Math.Min(minX, glyph.Destination.Left);
                        maxX = Math.Max(maxX, glyph.Destination.Right);
                    }

                    if(minX.Equals(float.PositiveInfinity)) return 0;

                    lineWidth += maxX - minX;
                    
                    width = Math.Max(width, lineWidth);
                }

                return width;
            }
        }

        public TextRenderer() {
            _lines.Add(new List<RenderedTextGlyph>());
            _currentLine = _lines[0];
        }

        public void Start(Font font, string text) {
            _font = font;
            _text = text;
            _index = 0;
            _cursor = Point.Zero;
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

            output.Font = _font;
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
            GlyphInfo glyphInfo = _font[c];
            if(glyphInfo.NotNull) {
                
                RenderedTextGlyph glyph = new RenderedTextGlyph {
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

            return null;
        }

        public void DrawOnto(RenderSurface surface, Point start, Color color, float size = 1) {
            for(int i = 0; i < _linesRendered; i++) {
                foreach(RenderedTextGlyph glyph in _lines[i]) {
                    glyph.DrawOnto(surface, start, color, size, _font);
                }
            }
        }
    }
}