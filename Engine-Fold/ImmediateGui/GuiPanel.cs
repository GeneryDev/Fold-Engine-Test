using System;
using System.Collections.Generic;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Graphics;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.ImmediateGui;

public class GuiPanel : GuiElement
{
    private const int ScrollAmount = 18;

    private readonly List<GuiElement> _children = new List<GuiElement>();

    private readonly GuiElement[] _pressedElements = new GuiElement[MouseEvent.MaxButtons];
    public readonly ObjectPoolCollection<GuiElement> ElementPool = new ObjectPoolCollection<GuiElement>();

    private long _lastFrameRendered;

    private GuiElement _previousElement;
    public Point ContentSize = Point.Zero;
    public bool Focused = true;

    public Point LayoutPosition = Point.Zero;
    public bool MayScroll = false;

    public ButtonAction MouseLeft = ButtonAction.Default;
    public ButtonAction MouseRight = ButtonAction.Default;
    public Dictionary<string, RenderedText> RenderedStrings = new Dictionary<string, RenderedText>();

    public Point ScrollPosition = Point.Zero;

    public GuiPanel(GuiEnvironment environment)
    {
        Environment = environment;
    }

    public GuiEnvironment Environment { get; }
    public bool Visible => _lastFrameRendered >= Time.TotalFrames - 1;

    public virtual void Reset()
    {
        _children.Clear();
        ElementPool.FreeAll();
        LayoutPosition = Bounds.Location;
        ContentSize = Point.Zero;
        _previousElement = null;
    }

    public void ResetLayoutPosition()
    {
        EndPreviousElement();
        LayoutPosition = Bounds.Location;
    }

    private T NewElement<T>() where T : GuiElement, new()
    {
        T element = null;
        element = ElementPool.Claim<T>();

        element.Parent = this;

        PrepareElement(element);
        return element;
    }

    private void PrepareElement(GuiElement element)
    {
        _children.Add(element);
        element.Bounds.Location = LayoutPosition - ScrollPosition;
        element.Reset(this);
        _previousElement = element;
    }

    protected void EndPreviousElement()
    {
        if (_previousElement != null)
        {
            _previousElement.AdjustSpacing(this);
            _previousElement.Displace(ref LayoutPosition);
            ContentSize.X = Math.Max(ContentSize.X,
                _previousElement.Bounds.Right - Bounds.Location.X + ScrollPosition.X);
            ContentSize.Y = Math.Max(ContentSize.Y,
                _previousElement.Bounds.Bottom - Bounds.Location.Y + ScrollPosition.Y);
            _previousElement = null;
        }
    }

    public T Element<T>() where T : GuiElement, new()
    {
        EndPreviousElement();
        var element = NewElement<T>();
        return element;
    }

    public T Element<T>(T existing) where T : GuiElement
    {
        EndPreviousElement();
        existing.Parent = this;
        PrepareElement(existing);
        return existing;
    }

    public GuiLabel Label(string label, int fontSize)
    {
        EndPreviousElement();
        var element = NewElement<GuiLabel>();
        element.Text(label);
        element.FontSize(fontSize);
        return element;
    }

    public GuiButton Button(string label, int fontSize)
    {
        EndPreviousElement();
        var element = NewElement<GuiButton>();
        element.Text(label);
        element.FontSize(fontSize);
        return element;
    }

    public GuiSeparator Separator(string label, int fontSize)
    {
        EndPreviousElement();
        var element = NewElement<GuiSeparator>();
        element.Label(label).FontSize(fontSize);
        return element;
    }

    public GuiSeparator Separator()
    {
        EndPreviousElement();
        var element = NewElement<GuiSeparator>();
        return element;
    }

    public GuiSpacing Spacing(int size)
    {
        EndPreviousElement();
        var element = NewElement<GuiSpacing>();
        element.Size = size;
        return element;
    }


    public RenderedText RenderString(string str, float size, bool useCache = true)
    {
        if (!RenderedStrings.ContainsKey(str))
        {
            Environment.Renderer.Fonts["default"].RenderString(str, out RenderedText rendered, size);
            if (useCache) RenderedStrings[str] = rendered;
            return rendered;
        }

        return RenderedStrings[str] = RenderedStrings[str].Update();
    }

    public RenderedText DrawString(string str, RenderSurface surface, Point start, Color color, float size)
    {
        Environment.Renderer.Fonts["default"].DrawString(str, surface, start, color, size);
        // if(!RenderedStrings.ContainsKey(str)) {
        // if(useCache) RenderedStrings[str] = rendered;
        // return rendered;
        // }
        return RenderedStrings[str];
    }

    public override void Focus()
    {
        Environment?.SetFocusedElement(Focusable ? this : null);
    }

    public override void Reset(GuiPanel parent)
    {
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        EndPreviousElement();

        if (Bounds.Contains(Environment.MousePos) && Environment is EditorEnvironment editorEnvironment)
        {
            editorEnvironment.HoverTarget.Element = this;
            if (MayScroll) editorEnvironment.HoverTarget.ScrollablePanel = this;
        }

        _lastFrameRendered = Time.TotalFrames;
        foreach (GuiElement element in _children)
            if (element.Bounds.Intersects(Bounds))
                element.Render(renderer, layer);
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            GuiElement element = _children[i];
            if (element.Bounds.Contains(e.Position))
            {
                _pressedElements[e.Button] = element;
                _pressedElements[e.Button].OnMousePressed(ref e);
                break;
            }
        }

        base.OnMousePressed(ref e);
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        _pressedElements[e.Button]?.OnMouseReleased(ref e);
        _pressedElements[e.Button] = null;
    }

    public bool IsPressed(GuiElement element, int buttonType = -1)
    {
        if (buttonType == -1)
        {
            foreach (GuiElement pressedElement in _pressedElements)
                if (pressedElement == element)
                    return true;

            return false;
        }

        return element == _pressedElements[buttonType];
    }

    public virtual void Scroll(int dir)
    {
        if (MayScroll)
        {
            ScrollPosition.Y -= dir * ScrollAmount;
            if (ScrollPosition.Y > ContentSize.Y - Bounds.Height) ScrollPosition.Y = ContentSize.Y - Bounds.Height;
            if (ScrollPosition.Y < 0) ScrollPosition.Y = 0;
            Environment.DismissPopups();
        }
    }

    public bool IsAncestorOf(GuiElement element)
    {
        while (true)
        {
            if (element == null) return false;
            if (this == element) return true;
            if (element.Parent == this) return true;
            if (element.Parent == null) return false;
            element = element.Parent;
        }
    }
}