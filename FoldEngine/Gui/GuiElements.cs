using System;
using EntryProject.Util;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui {
    public abstract class GuiElement {
        internal GuiPanel Parent;

        public virtual Point Displacement => new Point(0, Bounds.Height + Margin);

        public virtual GuiEnvironment Environment => Parent.Environment;

        public Rectangle Bounds;
        public int Margin = 8;

        public abstract void Reset(GuiPanel parent);
        public abstract void AdjustSpacing(GuiPanel parent);

        
        public abstract void Render(IRenderingUnit renderer, IRenderingLayer layer);

        
        public virtual void OnMousePressed(ref MouseEvent e) {
            if(!e.Consumed && ClickToFocus) {
                Focus();
                if(Focusable) e.Consumed = true;
            }
        }
        public virtual void OnMouseReleased(ref MouseEvent e) {}
        
        public virtual void OnKeyPressed(ref MouseEvent e) {}
        public virtual void OnKeyReleased(ref KeyboardEvent e) {}
        public virtual void OnKeyTyped(ref KeyboardEvent e) {}

        public virtual void OnFocusGained() { }
        public virtual void OnFocusLost() {}
        
        public virtual bool ClickToFocus => true;
        public virtual bool Focusable => false;

        public void Focus() {
            Parent?.Environment?.SetFocusedElement(Focusable ? this : null);
        }
        

        public bool Pressed(int buttonType = -1) {
            return Parent.IsPressed(this, buttonType);
        }
        public bool Rollover => Parent.Environment.HoverTargetPrevious.Element == this;
        public bool Focused => Parent?.Environment?.FocusOwner == this;
    }

    public class GuiLabel : GuiElement {
        protected string _text;
        protected int _fontSize = 14;
        protected int _textAlignment = 0;
        protected int _textMargin = 4;
        protected ITexture _icon = null;
        protected Point _iconSize;
        protected Color _textColor;
        protected bool _shouldCache = true;

        public override void Reset(GuiPanel parent) {
            _fontSize = 2;
            _textAlignment = 0;
            _textMargin = 4;
            _shouldCache = true;
            _textColor = Color.White;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * _fontSize / 7;
            Margin = 0;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            RenderedText renderedText = _shouldCache ? Parent.RenderString(_text, _fontSize) : default;
            if(!renderedText.HasValue) TextRenderer.Instance.Start(renderer.Fonts["default"], _text, _fontSize);

            float textWidth = renderedText.HasValue ? renderedText.Width : TextRenderer.Instance.Width;

            int totalWidth = (int) (textWidth);
            if(_icon != null) {
                totalWidth += _iconSize.X;
                totalWidth += 8;
            }

            int x = Margin;
            switch(_textAlignment) {
                case -1:
                    x = Bounds.X + _textMargin;
                    break;
                case 0:
                    x = (int) (Bounds.Center.X - totalWidth / 2);
                    break;
                case 1:
                    x = (int) (Bounds.X + Bounds.Width - totalWidth - _textMargin);
                    break;
                default:
                    break;
            }

            if(_icon != null) {
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = _icon,
                    DestinationRectangle = new Rectangle(x, Bounds.Center.Y - _iconSize.Y/2,  _iconSize.X, _iconSize.Y)
                });
                x += _iconSize.X;
                x += 8;
            }
            if(renderedText.HasValue) renderedText.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + _fontSize / 2), _textColor);
            else TextRenderer.Instance.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + 3 * _fontSize / 7), _textColor);
        }

        public GuiLabel Text(string text) {
            _text = text;
            return this;
        }

        public GuiLabel TextColor(Color textColor) {
            _textColor = textColor;
            return this;
        }

        public GuiLabel FontSize(int fontSize) {
            _fontSize = fontSize;
            return this;
        }

        public GuiLabel TextAlignment(int alignment) {
            _textAlignment = alignment;
            return this;
        }

        public GuiLabel TextMargin(int textMargin) {
            _textMargin = textMargin;
            return this;
        }

        public GuiLabel Icon(ITexture icon) {
            _icon = icon;
            _iconSize = new Point(icon.Width, icon.Height);
            return this;
        }

        public GuiLabel UseTextCache(bool shouldCache) {
            _shouldCache = shouldCache;
            return this;
        }
    }

    public class GuiButton : GuiLabel {

        private PooledValue<IGuiAction> _leftAction;
        private PooledValue<IGuiAction> _rightAction;

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            _leftAction.Free();
            _rightAction.Free();
        }

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 4;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = Pressed(MouseEvent.LeftButton) ? new Color(63, 63, 70) : Rollover ? Color.CornflowerBlue : new Color(37, 37, 38),
                DestinationRectangle = Bounds
            });
            base.Render(renderer, layer);
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            if(Bounds.Contains(e.Position)) {
                switch(e.Button) {
                    case MouseEvent.LeftButton: {
                        _leftAction.Value?.Perform(this, e);
                        break;
                    }
                    case MouseEvent.RightButton: {
                        _rightAction.Value?.Perform(this, e);
                        break;
                    }
                }
            }
        }

        

        public new GuiButton Text(string text) {
            base.Text(text);
            return this;
        }

        public new GuiButton FontSize(int fontSize) {
            base.FontSize(fontSize);
            return this;
        }

        public new GuiButton TextAlignment(int alignment) {
            base.TextAlignment(alignment);
            return this;
        }

        public new GuiButton TextMargin(int textMargin) {
            base.TextMargin(textMargin);
            return this;
        }

        public new GuiButton Icon(ITexture icon) {
            base.Icon(icon);
            return this;
        }

        public GuiButton LeftAction(IGuiAction action) {
            _leftAction.Value = action;
            return this;
        }

        public T LeftAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _leftAction.Value = action;
            return action;
        }

        public GuiButton RightAction(IGuiAction action) {
            _rightAction.Value = action;
            return this;
        }

        public T RightAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _rightAction.Value = action;
            return action;
        }
    }

    public class GuiSpacing : GuiElement {
        public int Size = 2;

        public override void Reset(GuiPanel parent) {
            Size = 2;
        }
        
        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = Size;
            Bounds.Height = Size;
            Margin = 0;
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
        }
    }

    public class GuiSeparator : GuiElement {
        protected string _label;
        protected int _fontSize = 7;
        protected int _thickness = 2;
        

        public override void Reset(GuiPanel parent) {
            _label = null;
            _fontSize = 2;
            _thickness = 2;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = _label != null ? 12 * _fontSize / 7 : _thickness;
            Margin = 4;
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            var color = new Color(45, 45, 48);
            if(_label != null) {
                RenderedText rendered = Parent.RenderString(_label, _fontSize);

                int lineWidth = (int) ((Bounds.Width - rendered.Width * _fontSize) / 2) - 2 * _fontSize;
                
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = color,
                    DestinationRectangle = new Rectangle(Bounds.X, Bounds.Center.Y - _thickness/2, lineWidth, _thickness)
                });
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = color,
                    DestinationRectangle = new Rectangle(Bounds.Right - lineWidth, Bounds.Center.Y - _thickness/2, lineWidth, _thickness)
                });
                
                rendered.DrawOnto(layer.Surface, new Point((int) (Bounds.Center.X - rendered.Width * _fontSize / 2), Bounds.Center.Y + 3 * _fontSize), Color.White);
            } else {
                int lineWidth = Bounds.Width;
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = color,
                    DestinationRectangle = new Rectangle(Bounds.Right - lineWidth, Bounds.Center.Y - _thickness/2, lineWidth, _thickness)
                });
            }
        }

        public GuiSeparator FontSize(int fontSize) {
            _fontSize = fontSize;
            return this;
        }

        public GuiSeparator Thickness(int thickness) {
            _thickness = thickness;
            return this;
        }

        public GuiSeparator Label(string label) {
            _label = label;
            return this;
        }
    }
}