using System;
using FoldEngine.Editor.Gui.Fields.Text;
using FoldEngine.Gui;

namespace FoldEngine.Editor.Gui.Fields {
    public class ValueDropdown : GuiButton {

        private int _parentWidthOccupied = 0;
        private int _fieldsInRow = 1;
        
        public ValueDropdown FieldSpacing(int parentWidthOccupied, int fieldsInRow = 1) {
            _parentWidthOccupied = parentWidthOccupied;
            _fieldsInRow = fieldsInRow;
            return this;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = (int)Math.Ceiling((float)(parent.Bounds.Width - _parentWidthOccupied) / _fieldsInRow);
            Bounds.Height = 18;
            Margin = 4;
        }
    }
}