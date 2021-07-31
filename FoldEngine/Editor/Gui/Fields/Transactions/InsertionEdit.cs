using FoldEngine.Editor.Gui.Fields.Text;

namespace FoldEngine.Editor.Gui.Fields.Transactions {
    public class InsertionEdit : DocumentTransactionBase {
        private char[] _textToInsert;
        
        public InsertionEdit(char[] value, TextField field) : base(field) {
            _textToInsert = value;
        }
        
        protected override void CalculateModifications() {
            foreach(Dot dot in PreviousProfile.Dots) {
                Modification(dot.Min, dot.Length, _textToInsert);
                Dot(dot.Min + _textToInsert.Length);
            }
        }
    }
}