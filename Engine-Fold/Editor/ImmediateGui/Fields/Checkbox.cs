﻿using System;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.ImmediateGui.Fields;

public class Checkbox : GuiElement, IInspectorField
{
    private bool _checked;
    private PooledValue<IGuiAction> _editedAction;
    private bool _editedValue;
    private long _editedTime;

    public bool EditValueForType(Type type, ref object value, int index)
    {
        value = !(bool)value;
        return true;
    }

    public override void Reset(GuiPanel parent)
    {
        _editedAction.Free();
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = Bounds.Height = 16;
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = new Color(63, 63, 70),
            DestinationRectangle = Bounds
        });
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = Pressed(MouseEvent.LeftButton) ? new Color(63, 63, 70) :
                Rollover ? Color.CornflowerBlue : new Color(37, 37, 38),
            DestinationRectangle = Bounds.Grow(-1)
        });

        if (_checked)
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = Environment.EditorResources.Get<Texture>(ref EditorIcons.Checkmark),
                DestinationRectangle = Bounds.Translate(offset)
            });
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (Bounds.Contains(e.Position))
            switch (e.Button)
            {
                case MouseEvent.LeftButton:
                {
                    _editedAction.Value?.Perform(this, e);
                    _editedTime = e.When;
                    _editedValue = !_checked;
                    break;
                }
            }
    }

    public Checkbox Value(bool value)
    {
        _checked = value;

        return this;
    }

    public bool IsChecked()
    {
        return Time.Now <= _editedTime + 1 ? _editedValue : _checked;
    }

    public Checkbox EditedAction(IGuiAction action)
    {
        _editedAction.Value = action;
        return this;
    }

    public T EditedAction<T>() where T : IGuiAction, new()
    {
        var action = Parent.Environment.ActionPool.Claim<T>();
        _editedAction.Value = action;
        return action;
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition += new Point(Bounds.Width, 0);
    }
}