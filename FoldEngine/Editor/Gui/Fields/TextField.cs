using System;
using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Gui.Fields {
    public class TextField : GuiElement {
        
        protected List<char> Buffer = new List<char>();
        private DocumentModel _document = new DocumentModel();
        private TextRenderer _textRenderer = new TextRenderer();
        private int _dot = 1;

        public override bool Focusable => true;

        private long _blinkerTime = 0;
        
        private const int FontSize = 9;
        

        public override void OnFocusGained() {
            _blinkerTime = Time.Now;
        }

        public TextField() {
            Buffer.Add('H');
            Buffer.Add('e');
            Buffer.Add('l');
            Buffer.Add('l');
            Buffer.Add('o');
            Buffer.Add(' ');
            Buffer.Add('W');
            Buffer.Add('o');
            Buffer.Add('r');
            Buffer.Add('l');
            Buffer.Add('d');
        }

        public override void Reset(GuiPanel parent) {
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * _document.GraphicalLines + 6;
            Margin = 4;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds
            });

            int x = Bounds.X + 4;
            int y = Bounds.Y + FontSize + 5;
            
            _document.Reset();
            _textRenderer.Start(renderer.Fonts["default"], "", FontSize);
            for(int i = 0; i <= Buffer.Count; i++) {
                if(i < Buffer.Count) {
                    char c = Buffer[i];
                    int prevX = _textRenderer.Cursor.X;
                    _textRenderer.Append(c);
                    if(c == '\n') {
                        _document.WriteBreak(true);
                    } else {
                        _document.WriteChar(_textRenderer.Cursor.X - prevX);
                    }
                }
            }

            _document.WriteEnd();

            if(Focused && BlinkerOn) {
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = Color.White,
                    DestinationRectangle = new Rectangle(x + _document.GetXForIndex(_dot) - 1, y + _document.GetYForIndex(_dot) - FontSize - 2, 1, FontSize + 4)
                });
            }
            
            
            float textWidth = _textRenderer.Width;

            int totalWidth = (int) (textWidth);


            _textRenderer.DrawOnto(layer.Surface, new Point(x, y), Focused ? Color.White : Color.LightGray);
        }

        public override void OnKeyTyped(ref KeyboardEvent e) {
            base.OnKeyTyped(ref e);

            Buffer.Insert(_dot++, e.Character);
        }

        public override void OnInput(ControlScheme controls) {
            if(controls.Get<ButtonAction>("editor.field.caret.left").Consume()) {
                _dot--;
                DotUpdated();
                ResetBlinker();
            }
            if(controls.Get<ButtonAction>("editor.field.caret.right").Consume()) {
                _dot++;
                DotUpdated();
                ResetBlinker();
            }
            if(controls.Get<ButtonAction>("editor.field.caret.debug").Consume()) {
                Console.WriteLine(_document.GetLogicalLineForIndex(_dot));
            }
        }

        private void DotUpdated() {
            _dot = Math.Max(0, Math.Min(Buffer.Count, _dot));
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            ResetBlinker();
            base.OnMouseReleased(ref e);
        }

        private void ResetBlinker() {
            _blinkerTime = Time.Now;
        }

        public bool BlinkerOn => Pressed() ||  ((Time.Now - _blinkerTime) / 500) % 2 == 0;
    }
}