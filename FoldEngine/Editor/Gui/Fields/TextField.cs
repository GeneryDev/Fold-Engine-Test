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
        private TextRenderer _textRenderer = new TextRenderer();
        
        public DocumentModel Document = new DocumentModel();
        public Caret Caret;

        public override bool Focusable => true;

        private const int FontSize = 9;

        public override void OnFocusGained() {
            Caret.OnFocusGained();
        }

        public TextField() {
            Caret = new Caret(this);
            
            Document.Buffer.Add('H');
            Document.Buffer.Add('e');
            Document.Buffer.Add('l');
            Document.Buffer.Add('l');
            Document.Buffer.Add('o');
            Document.Buffer.Add(' ');
            Document.Buffer.Add('W');
            Document.Buffer.Add('o');
            Document.Buffer.Add('r');
            Document.Buffer.Add('l');
            Document.Buffer.Add('d');
        }

        public override void Reset(GuiPanel parent) {
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * Document.GraphicalLines + 6;
            Margin = 4;
        }

        private Point TextRenderingStartPos => new Point(Bounds.X + 4, Bounds.Y + FontSize + 5);


        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds
            });

            _textRenderer.Start(renderer.Fonts["default"], "", FontSize);
            Document.Render(_textRenderer);

            if(Pressed(MouseEvent.LeftButton)) {
                Caret.DotIndex = Document.ViewToModel(Environment.MousePos - TextRenderingStartPos);
            }
            
            
            var textRenderingStartPos = TextRenderingStartPos;

            Caret.PreRender(renderer, layer, textRenderingStartPos);
            
            _textRenderer.DrawOnto(layer.Surface, textRenderingStartPos, Focused ? Color.White : Color.LightGray);

            Caret.PostRender(renderer, layer, textRenderingStartPos);
        }

        public override void OnMousePressed(ref MouseEvent e) {
            base.OnMousePressed(ref e);
            Caret.Dot = Document.ViewToModel(e.Position - TextRenderingStartPos);
        }

        public override void OnKeyTyped(ref KeyboardEvent e) {
            base.OnKeyTyped(ref e);

            Document.Buffer.Insert(Caret.Dot++, e.Character);
        }

        public override void OnInput(ControlScheme controls) {
            Caret.OnInput(controls);
            
            if(controls.Get<ButtonAction>("editor.field.caret.debug").Consume()) {
                // Console.WriteLine(_document.GetLogicalLineForIndex(_dot));
            }
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            base.OnMouseReleased(ref e);
        }
    }
}