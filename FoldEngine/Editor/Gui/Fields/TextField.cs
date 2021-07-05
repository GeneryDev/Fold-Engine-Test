using System;
using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Gui.Fields {
    public class TextField : GuiElement {
        
        protected List<char> Buffer = new List<char>();
        private int _dot = 1;

        public override bool Focusable => true;

        private long _blinkerTime = 0;

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
            Bounds.Height = 12 * 9 / 7;
            Margin = 4;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            TextRenderer textRenderer = TextRenderer.Instance;

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds
            });

            const int fontSize = 9;

            int x = Bounds.X + 4;
            int y = Bounds.Center.Y + fontSize / 2 + 1;
            
            textRenderer.Start(renderer.Fonts["default"], "", fontSize);
            for(int i = 0; i < Buffer.Count; i++) {
                char c = Buffer[i];
                if(i == _dot && Focused && BlinkerOn) {
                    layer.Surface.Draw(new DrawRectInstruction() {
                        Texture = renderer.WhiteTexture,
                        Color = Color.White,
                        DestinationRectangle = new Rectangle(x + textRenderer.Cursor.X - 1, y - fontSize - 2, 1, fontSize + 4)
                    });
                }
                textRenderer.Append(c);
            }

            
            float textWidth = textRenderer.Width;

            int totalWidth = (int) (textWidth);


            textRenderer.DrawOnto(layer.Surface, new Point(x, y), Focused ? Color.White : Color.LightGray);
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            _blinkerTime = Time.Now;
            base.OnMouseReleased(ref e);
        }

        public bool BlinkerOn => Pressed() ||  ((Time.Now - _blinkerTime) / 500) % 2 == 0;
    }
}