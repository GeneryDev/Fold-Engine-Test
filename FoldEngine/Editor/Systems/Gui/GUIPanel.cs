using System;
using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Editor.Systems {
    public class GuiPanel {
        public GuiEnvironment Environment;
        public Dictionary<string, RenderedText> RenderedStrings = new Dictionary<string,RenderedText>();
        public bool Focused = true;
        
        private long _lastFrameRendered;
        public bool Visible => _lastFrameRendered >= Time.Frame-1;
        
        public ButtonAction MouseLeft = ButtonAction.Default;
        public ButtonAction MouseRight = ButtonAction.Default;

        private List<GuiElement> _children = new List<GuiElement>();
        private List<GuiElement> _objectPool = new List<GuiElement>();
        private int _generation = 0;

        public Rectangle Bounds;
        
        private int x = 0;
        private int y = 0;

        public GuiPanel(GuiEnvironment environment) {
            Environment = environment;
        }

        public void Reset() {
            _children.Clear();
            _generation++;
            x = Bounds.X;
            y = Bounds.Y;
            _previousElement = null;
        }

        private T NewElement<T>() where T : GuiElement, new() {
            T element = null;
            foreach(GuiElement existing in _objectPool) {
                if(existing is T && existing.Generation != _generation) {
                    element = (T) existing;
                    break;
                }
            }

            if(element == null) {
                element = new T();
                element.Parent = this;
                _objectPool.Add(element);
            }

            element.Generation = this._generation;
            _children.Add(element);
            element.Bounds.X = x;
            element.Bounds.Y = y;
            element.Reset(this);
            _previousElement = element;
            return element;
        }

        private GuiElement _previousElement;

        private void EndPreviousElement() {
            if(_previousElement != null) {
                _previousElement.AdjustSpacing(this);
                y += _previousElement.Bounds.Height + _previousElement.Margin;        
            }
        }

        public GuiLabel Label(string label, int fontSize = 2) {
            EndPreviousElement();
            var element = NewElement<GuiLabel>();
            element.Text = label;
            element.FontSize(fontSize);
            return element;
        }

        public GuiButton Button(string label, int fontSize = 2) {
            EndPreviousElement();
            var element = NewElement<GuiButton>();
            element.Text = label;
            element.FontSize(fontSize);
            return element;
        }

        public GuiSeparator Separator(string label, int fontSize = 2) {
            EndPreviousElement();
            var element = NewElement<GuiSeparator>();
            element.Label(label).FontSize(fontSize);
            return element;
        }

        public GuiSeparator Separator() {
            EndPreviousElement();
            var element = NewElement<GuiSeparator>();
            return element;
        }

        public GuiSpacing Spacing(int size) {
            EndPreviousElement();
            var element = NewElement<GuiSpacing>();
            element.Size = size;
            return element;
        }

        public void End() {
            EndPreviousElement();
        }

        public RenderedText RenderString(string str, IRenderingUnit renderer) {
            if(!RenderedStrings.ContainsKey(str)) {
                renderer.Fonts["default"].RenderString(str, out RenderedText rendered);
                RenderedStrings[str] = rendered;
                return rendered;
            }
            return RenderedStrings[str];
        }

        public void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            _lastFrameRendered = Time.Frame;
            foreach(GuiElement element in _children) {
                element.Render(renderer, layer);
            }
        }

        private GuiElement _pressedElement;

        protected internal void OnMousePressed(Point pos) {
            for(int i = _children.Count - 1; i >= 0; i--) {
                GuiElement element = _children[i];
                if(element.Bounds.Contains(pos)) {
                    _pressedElement = element;
                    _pressedElement.OnMousePressed(pos);
                    break;
                }
            }
        }

        public void OnMouseReleased(Point pos) {
            _pressedElement?.OnMouseReleased(pos);
            _pressedElement = null;
        }

        public bool IsPressed(GuiElement guiElement) {
            return guiElement == _pressedElement;
        }
    }

    public abstract class GuiElement {
        internal GuiPanel Parent;
        internal int Generation;

        public bool Pressed => Parent.IsPressed(this);
        
        public Rectangle Bounds;
        public int Margin = 8;

        public abstract void Reset(GuiPanel parent);
        public abstract void AdjustSpacing(GuiPanel parent);

        public abstract void Render(IRenderingUnit renderer, IRenderingLayer layer);
        
        public virtual void OnMousePressed(Point pos) {}
        public virtual void OnMouseReleased(Point pos) {}
    }

    public class GuiLabel : GuiElement {
        public string Text;
        protected int _fontSize = 2;
        protected int _textAlignment = 0;
        protected int _textMargin = 4;
        protected ITexture _icon = null;
        protected Point _iconSize;

        public override void Reset(GuiPanel parent) {
            _fontSize = 2;
            _textAlignment = 0;
            _textMargin = 4;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * _fontSize;
            Margin = 0;
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            RenderedText rendered = Parent.RenderString(Text, renderer);

            int totalWidth = (int) (rendered.Width*_fontSize);
            if(_icon != null) {
                totalWidth += _iconSize.X;
                totalWidth += 8;
            }

            int x = Margin;
            switch(_textAlignment) {
                case -1:
                    x = Bounds.Left + _textMargin;
                    break;
                case 0:
                    x = (int) (Bounds.Center.X - totalWidth / 2);
                    break;
                case 1:
                    x = (int) (Bounds.Right - totalWidth - _textMargin);
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
            rendered.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + 3 * _fontSize), Color.White, _fontSize);
        }

        public GuiLabel FontSize(int fontSize) {
            this._fontSize = fontSize;
            return this;
        }

        public GuiLabel TextAlignment(int alignment) {
            this._textAlignment = alignment;
            return this;
        }

        public GuiLabel TextMargin(int textMargin) {
            this._textMargin = textMargin;
            return this;
        }

        public GuiLabel Icon(ITexture icon) {
            this._icon = icon;
            this._iconSize = new Point(icon.Width, icon.Height);
            return this;
        }
    }

    public class GuiButton : GuiLabel {

        private int _actionId;
        private long _data;

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 4;
        }

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            _actionId = 0;
            _data = 0;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            
            
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = Pressed ? new Color(63, 63, 70) : Bounds.Contains(Parent.Environment.MousePos) ? Color.CornflowerBlue : new Color(37, 37, 38),
                DestinationRectangle = Bounds
            });
            base.Render(renderer, layer);
        }

        public override void OnMouseReleased(Point pos) {
            if(Bounds.Contains(pos)) {
                Console.WriteLine(Text);
                if(_actionId != 0) {
                    Parent.Environment.PerformAction(_actionId, _data);
                }
            }
        }
        
        public GuiButton Action(int actionId, long data) {
            _actionId = actionId;
            _data = data;
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
        protected int _fontSize = 2;
        protected int _thickness = 2;
        

        public override void Reset(GuiPanel parent) {
            _label = null;
            _fontSize = 2;
            _thickness = 2;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * _fontSize;
            Margin = 4;
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            var color = new Color(45, 45, 48);
            if(_label != null) {
                RenderedText rendered = Parent.RenderString(_label, renderer);

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
                
                rendered.DrawOnto(layer.Surface, new Point((int) (Bounds.Center.X - rendered.Width * _fontSize / 2), Bounds.Center.Y + 3 * _fontSize), Color.White, _fontSize);
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
            this._fontSize = fontSize;
            return this;
        }

        public GuiSeparator Thickness(int thickness) {
            this._thickness = thickness;
            return this;
        }

        public GuiSeparator Label(string label) {
            this._label = label;
            return this;
        }
    }
}