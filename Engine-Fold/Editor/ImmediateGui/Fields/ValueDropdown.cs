using System;
using FoldEngine.ImmediateGui;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.ImmediateGui.Fields;

public class ValueDropdown : GuiButton
{
    private int _fieldsInRow = 1;

    private int _parentWidthOccupied;

    public ValueDropdown FieldSpacing(int parentWidthOccupied, int fieldsInRow = 1)
    {
        _parentWidthOccupied = parentWidthOccupied;
        _fieldsInRow = fieldsInRow;
        return this;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = (int)Math.Ceiling((float)(parent.Bounds.Width - _parentWidthOccupied) / _fieldsInRow);
        Bounds.Height = 18;
        Margin = 4;
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition += new Point(Bounds.Width, 0);
    }
}