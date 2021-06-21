using System;
using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Editor.Views {
    public class GuiPanel : GuiElement {
        private const int ScrollAmount = 18;
        
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

        public Point LayoutPosition = Point.Zero;
        public Point ContentSize = Point.Zero;
        
        public Point ScrollPosition = Point.Zero;
        public bool MayScroll = false;

        public GuiPanel(GuiEnvironment environment) {
            Environment = environment;
        }

        public void Reset() {
            _children.Clear();
            _generation++;
            LayoutPosition = Bounds.Location;
            ContentSize = Point.Zero;
            _previousElement = null;
        }

        public void ResetLayoutPosition() {
            EndPreviousElement();
            LayoutPosition = Bounds.Location;
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

            PrepareElement(element);
            return element;
        }

        private void PrepareElement(GuiElement element) {
            element.Generation = this._generation;
            _children.Add(element);
            element.Bounds.Location = LayoutPosition - ScrollPosition;
            element.Reset(this);
            _previousElement = element;
        }

        private GuiElement _previousElement;

        private void EndPreviousElement() {
            if(_previousElement != null) {
                _previousElement.AdjustSpacing(this);
                LayoutPosition += _previousElement.Displacement;
                ContentSize.X = Math.Max(ContentSize.X, _previousElement.Bounds.Right);
                ContentSize.Y = Math.Max(ContentSize.Y, _previousElement.Bounds.Bottom - Bounds.Location.Y + ScrollPosition.Y);
                _previousElement = null;
            }
        }

        public T Element<T>() where T : GuiElement, new() {
            EndPreviousElement();
            var element = NewElement<T>();
            return element;
        }

        public T Element<T>(T existing) where T : GuiElement {
            EndPreviousElement();
            existing.Parent = this;
            PrepareElement(existing);
            return existing;
        }

        public GuiLabel Label(string label, int fontSize) {
            EndPreviousElement();
            var element = NewElement<GuiLabel>();
            element.Text(label);
            element.FontSize(fontSize);
            return element;
        }

        public GuiButton Button(string label, int fontSize) {
            EndPreviousElement();
            var element = NewElement<GuiButton>();
            element.Text(label);
            element.FontSize(fontSize);
            return element;
        }

        public GuiSeparator Separator(string label, int fontSize) {
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


        public RenderedText RenderString(string str, float size, bool useCache = true) {
            if(!RenderedStrings.ContainsKey(str)) {
                Environment.Renderer.Fonts["default"].RenderString(str, out RenderedText rendered, size);
                if(useCache) RenderedStrings[str] = rendered;
                return rendered;
            }
            return RenderedStrings[str];
        }
        public RenderedText DrawString(string str, RenderSurface surface, Point start, Color color, float size) {
            Environment.Renderer.Fonts["default"].DrawString(str, surface, start, color, size);
            // if(!RenderedStrings.ContainsKey(str)) {
                // if(useCache) RenderedStrings[str] = rendered;
                // return rendered;
            // }
            return RenderedStrings[str];
        }

        public override void Reset(GuiPanel parent) {
        }

        public override void AdjustSpacing(GuiPanel parent) {
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            EndPreviousElement();

            if(MayScroll && Bounds.Contains(Environment.MousePos) && Environment is EditorEnvironment editorEnvironment) {
                editorEnvironment.HoverTarget.ScrollablePanel = this;
            }
            _lastFrameRendered = Time.Frame;
            foreach(GuiElement element in _children) {
                if(element.Bounds.Intersects(Bounds)) element.Render(renderer, layer);
            }
        }

        private GuiElement _pressedElement;

        public override void OnMousePressed(Point pos) {
            for(int i = _children.Count - 1; i >= 0; i--) {
                GuiElement element = _children[i];
                if(element.Bounds.Contains(pos)) {
                    _pressedElement = element;
                    _pressedElement.OnMousePressed(pos);
                    break;
                }
            }
        }

        public override void OnMouseReleased(Point pos) {
            _pressedElement?.OnMouseReleased(pos);
            _pressedElement = null;
        }

        public bool IsPressed(GuiElement guiElement) {
            return guiElement == _pressedElement;
        }

        public void Scroll(int dir) {
            if(MayScroll) {
                ScrollPosition.Y -= dir * ScrollAmount;
                if(ScrollPosition.Y > ContentSize.Y - Bounds.Height) ScrollPosition.Y = ContentSize.Y - Bounds.Height;
                if(ScrollPosition.Y < 0) ScrollPosition.Y = 0;
            }
        }
    }

    public abstract class GuiElement {
        internal GuiPanel Parent;
        internal int Generation;

        public bool Pressed => Parent.IsPressed(this);
        public virtual Point Displacement => new Point(0, Bounds.Height + Margin);

        public Rectangle Bounds;
        public int Margin = 8;

        public abstract void Reset(GuiPanel parent);
        public abstract void AdjustSpacing(GuiPanel parent);

        public abstract void Render(IRenderingUnit renderer, IRenderingLayer layer);
        
        public virtual void OnMousePressed(Point pos) {}
        public virtual void OnMouseReleased(Point pos) {}
    }

    public class GuiLabel : GuiElement {
        protected string _text;
        protected int _fontSize = 14;
        protected int _textAlignment = 0;
        protected int _textMargin = 4;
        protected ITexture _icon = null;
        protected Point _iconSize;
        protected bool _shouldCache = true;

        public override void Reset(GuiPanel parent) {
            _fontSize = 2;
            _textAlignment = 0;
            _textMargin = 4;
            _shouldCache = true;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = 12 * _fontSize / 7;
            Margin = 0;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {

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
            if(renderedText.HasValue) renderedText.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + _fontSize / 2), Color.White);
            else TextRenderer.Instance.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + 3 * _fontSize / 7), Color.White);
        }

        public GuiLabel Text(string text) {
            this._text = text;
            return this;
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

        public GuiLabel UseTextCache(bool shouldCache) {
            _shouldCache = shouldCache;
            return this;
        }
    }

    public class GuiButton : GuiLabel {

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 4;
        }

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
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
                PerformAction(pos);
            }
        }

        public virtual void PerformAction(Point point) {
            
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