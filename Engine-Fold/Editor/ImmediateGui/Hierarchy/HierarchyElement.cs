﻿using System;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.ImmediateGui.Hierarchy;

public class HierarchyElement<TI> : GuiLabel
{
    public enum HierarchyEventType
    {
        None,
        Expand,
        Down,
        Up,
        Context
    }

    protected bool _depress;
    protected int _depth;

    protected bool _dragging;
    protected bool _expandable;

    internal Hierarchy<TI> _hierarchy;

    protected TI _id;

    private HierarchyEvent _lastEvent;
    protected bool _selected;

    protected Rectangle ExpandBounds;

    protected virtual Color NormalColor => new Color(37, 37, 38);
    protected virtual Color RolloverColor => new Color(63, 63, 70);
    protected virtual Color PressedColor => new Color(63, 63, 70);

    protected virtual Color SelectedColor => Color.CornflowerBlue;

    public override void Reset(GuiPanel parent)
    {
        base.Reset(parent);
        FontSize(14);
        TextAlignment(-1);
        _selected = false;
        _hierarchy = null;
        _id = default;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        base.AdjustSpacing(parent);
        Margin = 0;
        ExpandBounds = new Rectangle(Bounds.Left + 4 + 16 * _depth,
            Bounds.Top + Bounds.Height / 2 - 16 / 2, 16,
            16);
    }

    public HierarchyElement<TI> Entity(Entity entity, int depth = 0)
    {
        if (entity.EntityId is TI id)
        {
            Id(id);

            Text(entity.Name);
            TextMargin(4 + 16 * (depth + 1) + 4);
            _expandable = entity.Hierarchical.FirstChildId != -1;
            _depth = depth;
        }
        else
        {
            throw new ArgumentException(
                "Cannot run HierarchyElement.Entity on an element whose ID Type is not long");
        }

        return this;
    }

    public HierarchyElement<TI> Id(TI id)
    {
        _id = id;
        return this;
    }

    public new HierarchyElement<TI> Icon(ITexture icon)
    {
        base.Icon(icon);
        return this;
    }

    public new HierarchyElement<TI> Icon(ITexture icon, Color color)
    {
        base.Icon(icon, color);
        return this;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (_depress)
        {
            _depress = false;
            _hierarchy.Pressed = false;
        }

        if (Bounds.Contains(Environment.MousePos))
        {
            Environment.HoverTarget.Element = this;
        }

        if (Pressed(MouseEvent.LeftButton) && Environment.HoverTargetPrevious.Element != this && _hierarchy.CanDrag)
        {
            _dragging = true;
            if (_hierarchy != null) _hierarchy.Dragging = true;
        }

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = _selected ? SelectedColor :
                Pressed(MouseEvent.LeftButton) ? PressedColor :
                Rollover ? RolloverColor : NormalColor,
            DestinationRectangle = Bounds.Translate(offset)
        });

        if (_expandable)
        {
            ITexture triangleTexture =
                Environment.EditorResources.Get<Texture>(
                    ref _hierarchy?.IsExpanded(_id) ?? false
                        ? ref EditorIcons.TriangleDown
                        : ref EditorIcons.TriangleRight, null);
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = triangleTexture,
                DestinationRectangle = ExpandBounds.Translate(offset)
            });
        }

        base.Render(renderer, layer, offset);

        if (_hierarchy != null && _hierarchy.Dragging && Environment.HoverTarget.Element == this)
        {
            int relative = 0;
            if (Environment.MousePos.Y <= Bounds.Top + Bounds.Height / (_hierarchy.CanDragInto ? 3 : 2))
                relative = -1;
            else if (Environment.MousePos.Y > Bounds.Bottom - Bounds.Height / (_hierarchy.CanDragInto ? 3 : 2))
                relative = 1;
            _hierarchy.DragTargetId = _id;
            _hierarchy.DragRelative = relative;

            if (relative != 0)
            {
                int lineCenterY = Bounds.Center.Y + Bounds.Height / 2 * relative;
                int x = 4 + 16 * (_depth + 1) + 4;
                var lineBounds = new Rectangle(Bounds.Left + x, lineCenterY, Bounds.Width - x, 0);
                _hierarchy.DragLine = lineBounds;
            }
        }

        // if (_dragging) _hierarchy?.DrawDragLine(renderer, layer);
    }

    public HierarchyEventType GetEvent(out Point p)
    {
        HierarchyEventType rv = GetEvent();
        p = rv != HierarchyEventType.None ? _lastEvent.Event.Position : default;
        return rv;
    }

    public HierarchyEventType GetEvent()
    {
        if (_lastEvent.Type != HierarchyEventType.None
            && _lastEvent.Event.When != 0
            && Time.Now >= _lastEvent.Event.When
            && !_lastEvent.Event.Consumed)
        {
            _lastEvent.Event.Consumed = true;
            return _lastEvent.Type;
        }

        return HierarchyEventType.None;
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        if (e.Button == MouseEvent.LeftButton)
        {
            _hierarchy.Pressed = true;
            _hierarchy.DragTargetId = _hierarchy.DefaultId;
            if (_expandable && ExpandBounds.Contains(e.Position))
            {
            }
            else
            {
                _lastEvent = new HierarchyEvent(e, HierarchyEventType.Down);
            }
        }
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (_dragging && e.Button == MouseEvent.LeftButton)
        {
            _hierarchy.Drop();
            _dragging = false;
            if (_hierarchy != null) _hierarchy.Dragging = false;
        }

        if (Bounds.Contains(e.Position))
            switch (e.Button)
            {
                case MouseEvent.LeftButton:
                {
                    if (_expandable && ExpandBounds.Contains(e.Position))
                        _lastEvent = new HierarchyEvent(e, HierarchyEventType.Expand);
                    else
                        _lastEvent = new HierarchyEvent(e, HierarchyEventType.Up);
                    break;
                }
                case MouseEvent.RightButton:
                {
                    _lastEvent = new HierarchyEvent(e, HierarchyEventType.Context);
                    break;
                }
            }

        _depress = true;
    }

    public HierarchyElement<TI> Selected(bool selected)
    {
        _selected = selected;
        return this;
    }

    public HierarchyElement<TI> Hierarchy(Hierarchy<TI> hierarchy)
    {
        _hierarchy = hierarchy;
        return this;
    }

    public struct HierarchyEvent
    {
        public MouseEvent Event;
        public readonly HierarchyEventType Type;

        public HierarchyEvent(MouseEvent e, HierarchyEventType type)
        {
            Event = e;
            Type = type;
        }
    }
}