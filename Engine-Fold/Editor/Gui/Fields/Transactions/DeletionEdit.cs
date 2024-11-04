using System;
using FoldEngine.Editor.Gui.Fields.Text;

namespace FoldEngine.Editor.Gui.Fields.Transactions;

public class DeletionEdit : DocumentTransactionBase
{
    private readonly bool _forward;
    private readonly bool _wholeWord;

    public DeletionEdit(TextField field, bool wholeWord, bool forward = false) : base(field)
    {
        _wholeWord = wholeWord;
        _forward = forward;
    }

    protected override void CalculateModifications()
    {
        foreach (Dot dot in PreviousProfile.Dots)
            if (dot.IsPoint)
            {
                int end;
                // Hello World
                if (_forward)
                    end = _wholeWord ? dot.GetPositionAfterWord() : dot.GetPositionAfter();
                else
                    end = _wholeWord ? dot.GetPositionBeforeWord() : dot.GetPositionBefore();
                Modification(Math.Min(dot.Index, end), Math.Abs(dot.Index - end), Array.Empty<char>());
                Dot(Math.Min(dot.Index, end));
            }
            else
            {
                Modification(dot.Min, dot.Length, Array.Empty<char>());
                Dot(dot.Min);
            }
    }
}