using FoldEngine.Graphics;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.ImmediateGui;

public abstract class GuiElement
{
    public Rectangle Bounds;
    public int Margin = 8;
    internal GuiPanel Parent;

    public virtual GuiEnvironment Environment => Parent.Environment;
    public virtual Scene Scene => Environment.Scene;

    public virtual bool ClickToFocus => true;
    public virtual bool Focusable => false;
    public bool Rollover => Parent.Environment.HoverTargetPrevious.Element == this;
    public bool Focused => Parent?.Environment?.FocusOwner == this;

    public abstract void Reset(GuiPanel parent);
    public abstract void AdjustSpacing(GuiPanel parent);


    public abstract void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default);


    public virtual void OnMousePressed(ref MouseEvent e)
    {
        if (!e.Consumed && ClickToFocus)
        {
            Focus();
            if (Focusable) e.Consumed = true;
        }
    }

    public virtual void OnMouseReleased(ref MouseEvent e)
    {
    }

    public virtual void OnKeyTyped(ref KeyboardEvent e)
    {
    }

    public virtual void OnInput(ControlScheme controls)
    {
    }

    public virtual void OnFocusGained()
    {
    }

    public virtual void OnFocusLost()
    {
    }

    public virtual void Focus()
    {
        Parent?.Environment?.SetFocusedElement(Focusable ? this : null);
    }


    public bool Pressed(int buttonType = -1)
    {
        return Parent.IsPressed(this, buttonType);
    }

    public virtual void Displace(ref Point layoutPosition)
    {
        layoutPosition += new Point(0, Bounds.Height + Margin);
    }
}

public class GuiLabel : GuiElement
{
    protected int _fontSize = 14;
    protected ITexture _icon;
    protected Color _iconColor = Color.White;
    protected Point _iconSize;
    protected bool _shouldCache = true;
    protected string _text;
    protected int _textAlignment;
    protected Color _textColor;
    protected int _textMargin = 4;

    public override void Reset(GuiPanel parent)
    {
        _fontSize = 14;
        _icon = null;
        _iconColor = Color.White;
        _iconSize = default;
        _shouldCache = true;
        _text = null;
        _textAlignment = 0;
        _textColor = Color.White;
        _textMargin = 4;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = parent.Bounds.Width;
        Bounds.Height = 12 * _fontSize / 7;
        Margin = 0;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        RenderedText renderedText = _shouldCache ? Parent.RenderString(_text, _fontSize) : default;
        if (!renderedText.HasValue) TextRenderer.Instance.Start(renderer.Fonts["default"], _text, _fontSize);

        float textWidth = renderedText.HasValue ? renderedText.Width : TextRenderer.Instance.Width;

        int totalWidth = (int)textWidth;
        if (_icon != null)
        {
            totalWidth += _iconSize.X;
            totalWidth += 8;
        }

        int x = Margin;
        switch (_textAlignment)
        {
            case -1:
                x = Bounds.X + _textMargin;
                break;
            case 0:
                x = Bounds.Center.X - totalWidth / 2;
                break;
            case 1:
                x = Bounds.X + Bounds.Width - totalWidth - _textMargin;
                break;
        }

        if (_icon != null)
        {
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = _icon,
                Color = _iconColor,
                DestinationRectangle = new Rectangle(_text.Length > 0 ? x : Bounds.Center.X - _iconSize.X / 2,
                    Bounds.Center.Y - _iconSize.Y / 2, _iconSize.X, _iconSize.Y).Translate(offset)
            });
            x += _iconSize.X;
            x += 8;
        }

        if (renderedText.HasValue)
            renderedText.DrawOnto(layer.Surface, new Point(x, Bounds.Center.Y + _fontSize / 2) + offset,
                _textColor);
        else
            TextRenderer.Instance.DrawOnto(layer.Surface,
                new Point(x, Bounds.Center.Y + 3 * _fontSize / 7) + offset, _textColor);
    }

    public GuiLabel Text(string text)
    {
        _text = text;
        return this;
    }

    public GuiLabel TextColor(Color textColor)
    {
        _textColor = textColor;
        return this;
    }

    public GuiLabel FontSize(int fontSize)
    {
        _fontSize = fontSize;
        return this;
    }

    public GuiLabel TextAlignment(int alignment)
    {
        _textAlignment = alignment;
        return this;
    }

    public GuiLabel TextMargin(int textMargin)
    {
        _textMargin = textMargin;
        return this;
    }

    public GuiLabel Icon(ITexture icon, Color color)
    {
        _icon = icon;
        if (icon != null)
        {
            _iconColor = color;
            _iconSize = new Point(icon.Width, icon.Height);
        }

        return this;
    }

    public GuiLabel Icon(ITexture icon)
    {
        return Icon(icon, Color.White);
    }

    public GuiLabel UseTextCache(bool shouldCache)
    {
        _shouldCache = shouldCache;
        return this;
    }
}

public class GuiImage : GuiElement
{
    protected ITexture _image;
    protected Color _color = Color.White;
    protected Point _iconSize;

    public override void Reset(GuiPanel parent)
    {
        _image = null;
        _color = Color.White;
        _iconSize = Point.Zero;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Size = _iconSize;
        Margin = 0;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        if (_image != null)
        {
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = _image,
                Color = _color,
                DestinationRectangle = Bounds.Translate(offset)
            });
        }
    }

    public GuiImage Image(ITexture img, Color color, int? width, int? height = null)
    {
        _image = img;
        _color = color;
        if (width == null && height == null)
        {
            _iconSize = new Point(img.Width, img.Height);
        }
        else if (width != null && height != null)
        {
            _iconSize = new Point(width.Value, height.Value);
        }
        else if (width != null)
        {
            _iconSize = new Point(width.Value, (int)((float)img.Height / img.Width * width.Value));
        }
        else
        {
            _iconSize = new Point((int)((float)img.Width / img.Height * height.Value), height.Value);
        }

        return this;
    }
}

public class GuiButton : GuiLabel
{
    private MouseEvent _lastEvent;

    private PooledValue<IGuiAction> _leftAction;

    protected virtual Color NormalColor => new Color(37, 37, 38);
    protected virtual Color RolloverColor => Color.CornflowerBlue;
    protected virtual Color PressedColor => new Color(63, 63, 70);

    public override void Reset(GuiPanel parent)
    {
        base.Reset(parent);
        _leftAction.Free();
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        base.AdjustSpacing(parent);
        Margin = 4;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = Pressed(MouseEvent.LeftButton) ? PressedColor : Rollover ? RolloverColor : NormalColor,
            DestinationRectangle = Bounds.Translate(offset)
        });
        base.Render(renderer, layer, offset);
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (Bounds.Contains(e.Position))
            switch (e.Button)
            {
                case MouseEvent.LeftButton:
                {
                    _lastEvent = e;
                    _leftAction.Value?.Perform(this, e);
                    break;
                }
                case MouseEvent.RightButton:
                {
                    _lastEvent = e;
                    break;
                }
            }
    }


    public new GuiButton Text(string text)
    {
        base.Text(text);
        return this;
    }

    public new GuiButton FontSize(int fontSize)
    {
        base.FontSize(fontSize);
        return this;
    }

    public new GuiButton TextAlignment(int alignment)
    {
        base.TextAlignment(alignment);
        return this;
    }

    public new GuiButton TextMargin(int textMargin)
    {
        base.TextMargin(textMargin);
        return this;
    }

    public new GuiButton Icon(ITexture icon, Color color)
    {
        base.Icon(icon, color);
        return this;
    }

    public new GuiButton Icon(ITexture icon)
    {
        base.Icon(icon);
        return this;
    }

    public bool IsPressed(int button = MouseEvent.LeftButton)
    {
        if (_lastEvent.Button == button
            && _lastEvent.When != 0
            && Time.Now >= _lastEvent.When
            && !_lastEvent.Consumed)
        {
            _lastEvent.Consumed = true;
            return true;
        }

        return false;
    }

    public bool IsPressed(out Point position, int button = MouseEvent.LeftButton)
    {
        bool pressed = IsPressed(button);
        position = pressed ? _lastEvent.Position : default;
        return pressed;
    }

    public GuiButton LeftAction(IGuiAction action)
    {
        _leftAction.Value = action;
        return this;
    }

    public T LeftAction<T>() where T : IGuiAction, new()
    {
        var action = Parent.Environment.ActionPool.Claim<T>();
        _leftAction.Value = action;
        return action;
    }
}

public class GuiSpacing : GuiElement
{
    public int Size = 2;

    public override void Reset(GuiPanel parent)
    {
        Size = 2;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = Size;
        Bounds.Height = Size;
        Margin = 0;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
    }
}

public class GuiSeparator : GuiElement
{
    protected int _fontSize = 7;
    protected string _label;
    protected int _thickness = 2;


    public override void Reset(GuiPanel parent)
    {
        _label = null;
        _fontSize = 2;
        _thickness = 2;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = parent.Bounds.Width;
        Bounds.Height = _label != null ? 12 * _fontSize / 7 : _thickness;
        Margin = 4;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        var color = new Color(45, 45, 48);
        if (_label != null)
        {
            RenderedText rendered = Parent.RenderString(_label, _fontSize);

            int lineWidth = (Bounds.Width - rendered.Width * _fontSize) / 2 - 2 * _fontSize;

            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = color,
                DestinationRectangle =
                    new Rectangle(Bounds.X, Bounds.Center.Y - _thickness / 2, lineWidth, _thickness).Translate(
                        offset)
            });
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = color,
                DestinationRectangle = new Rectangle(Bounds.Right - lineWidth, Bounds.Center.Y - _thickness / 2,
                    lineWidth, _thickness).Translate(offset)
            });

            rendered.DrawOnto(layer.Surface,
                new Point(Bounds.Center.X - rendered.Width * _fontSize / 2, Bounds.Center.Y + 3 * _fontSize),
                Color.White);
        }
        else
        {
            int lineWidth = Bounds.Width;
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = color,
                DestinationRectangle = new Rectangle(Bounds.Right - lineWidth, Bounds.Center.Y - _thickness / 2,
                    lineWidth, _thickness).Translate(offset)
            });
        }
    }

    public GuiSeparator FontSize(int fontSize)
    {
        _fontSize = fontSize;
        return this;
    }

    public GuiSeparator Thickness(int thickness)
    {
        _thickness = thickness;
        return this;
    }

    public GuiSeparator Label(string label)
    {
        _label = label;
        return this;
    }
}