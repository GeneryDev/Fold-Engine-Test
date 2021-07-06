namespace FoldEngine.Editor.Gui.Fields.Transactions {
    public class DeletionEdit : DocumentTransactionBase {
        public DeletionEdit(TextField field) : base(field) { }
        
        protected override void CalculateModifications() {
            foreach(Dot dot in PreviousProfile.Dots) {
                if(dot.IsPoint) {
                    //TODO ctrl
                    
                    if(dot.Index > 0) {
                        Modification(dot.Index-1, 1, new char[0]);
                        Dot(dot.Index-1);
                    }
                } else {
                    Modification(dot.Min, dot.Length, new char[0]);
                    Dot(dot.Min);
                }
            }
        }
    }
}