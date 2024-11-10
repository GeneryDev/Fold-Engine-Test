using FoldEngine.Editor.ImmediateGui.Fields.Text;

namespace FoldEngine.Editor.ImmediateGui.Fields.Transactions;

public class InsertionEdit : DocumentTransactionBase
{
    private readonly char[] _textToInsert;

    public InsertionEdit(char[] value, TextField field) : base(field)
    {
        _textToInsert = value;
    }

    protected override void CalculateModifications()
    {
        foreach (Dot dot in PreviousProfile.Dots)
        {
            Modification(dot.Min, dot.Length, _textToInsert);
            Dot(dot.Min + _textToInsert.Length);
        }
    }
}