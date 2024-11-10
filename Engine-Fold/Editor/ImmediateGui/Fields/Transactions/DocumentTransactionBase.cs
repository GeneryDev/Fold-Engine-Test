using System;
using System.Collections.Generic;
using FoldEngine.Editor.ImmediateGui.Fields.Text;
using FoldEngine.Util;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.ImmediateGui.Fields.Transactions;

public abstract class DocumentTransactionBase : Transaction<TextField>
{
    private static readonly ObjectPool<List<DocumentModification>> TempModificationListPool =
        new ObjectPool<List<DocumentModification>>();

    private static readonly ObjectPool<List<Dot>> TempDotListPool = new ObjectPool<List<Dot>>();

    private DocumentModification[] _modifications;
    private List<Dot> _tempDotList;

    private List<DocumentModification> _tempModificationList;

    protected TextField Field;
    protected CaretProfile NextProfile;

    protected CaretProfile PreviousProfile;

    public DocumentTransactionBase(TextField field)
    {
        Field = field;
        PreviousProfile = field.Caret.CreateProfile();
    }

    protected abstract void CalculateModifications();

    protected void Modification(int start, int length, char[] newValue)
    {
        if (_tempModificationList == null)
            throw new InvalidOperationException(
                "Cannot add a modification outside the CalculateModifications method");

        var modification = new DocumentModification
        {
            Start = start,
            Length = length,
            OldValue = Field.Document.GetChars(start, length),
            NewValue = newValue
        };

        _tempModificationList.Add(modification);

        for (int i = 0; i < _tempDotList.Count; i++)
        {
            Dot dot = _tempDotList[i];
            if (start <= dot.Index && dot.Index <= start + length) dot.Index = start;
            if (start <= dot.Mark && dot.Mark <= start + length) dot.Mark = start;

            if (dot.Index >= start) dot.Index += newValue.Length;
            if (dot.Mark >= start) dot.Mark += newValue.Length;

            _tempDotList[i] = dot;
        }
    }

    protected void Dot(int index)
    {
        Dot(index, index);
    }

    protected void Dot(int index, int mark)
    {
        if (_tempModificationList == null)
            throw new InvalidOperationException("Cannot add a dot outside the CalculateModifications method");

        _tempDotList.Add(new Dot(Field.Document, index, mark));
    }

    public override bool Redo(TextField target)
    {
        if (_modifications == null)
        {
            _tempModificationList = TempModificationListPool.Claim();
            _tempDotList = TempDotListPool.Claim();

            CalculateModifications();
            _tempModificationList.Sort((a, b) => b.Start - a.Start);
            _modifications = _tempModificationList.ToArray();
            NextProfile = new CaretProfile(_tempDotList.ToArray());

            _tempModificationList.Clear();
            _tempDotList.Clear();

            TempModificationListPool.Free(_tempModificationList);
            TempDotListPool.Free(_tempDotList);

            _tempModificationList = null;
            _tempDotList = null;
        }

        bool actionPerformed = false;

        for (int i = _modifications.Length - 1; i >= 0; i--)
            actionPerformed |= _modifications[i].Redo(target.Document);

        target.Caret.SetProfile(NextProfile);

        return actionPerformed;
    }

    public override bool Undo(TextField target)
    {
        bool actionPerformed = false;

        for (int i = 0; i < _modifications.Length; i++) actionPerformed |= _modifications[i].Undo(target.Document);

        target.Caret.SetProfile(PreviousProfile);

        return actionPerformed;
    }
}

public struct DocumentModification
{
    public int Start;
    public int Length;
    public char[] OldValue;
    public char[] NewValue;

    public bool Redo(Document document)
    {
        document.Replace(Start, Length, NewValue);

        return Length > 0 || NewValue.Length > 0;
    }

    public bool Undo(Document document)
    {
        document.Replace(Start, NewValue.Length, OldValue);

        return Length > 0 || NewValue.Length > 0;
    }
}