using System;
using System.Collections.Generic;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui.Fields.Text;

public class Caret
{
    private long _blinkerTime;
    private readonly List<Dot> _dots = new List<Dot>();
    private readonly TextField _parent;

    public Caret(TextField parent)
    {
        _parent = parent;

        _dots.Add(new Dot(Document));
    }

    private Document Document => _parent.Document;
    public bool BlinkerOn => _parent.Pressed(MouseEvent.LeftButton) || (Time.Now - _blinkerTime) / 500 % 2 == 0;

    public int Dot
    {
        get => _dots[0].Index;
        set
        {
            Dot newDot = _dots[0];
            newDot.Index = newDot.Mark = value;
            _dots[0] = newDot;
        }
    }

    public int DotIndex
    {
        get => _dots[0].Index;
        set
        {
            Dot newDot = _dots[0];
            newDot.Index = value;
            _dots[0] = newDot;
        }
    }

    public int DotMark
    {
        get => _dots[0].Mark;
        set
        {
            Dot newDot = _dots[0];
            newDot.Mark = value;
            _dots[0] = newDot;
        }
    }

    public void OnInput(ControlScheme controls)
    {
        KeyModifiers modifiers = KeyModifiersExt.GetKeyModifiers();
        if (controls.Get<ButtonAction>("editor.field.caret.left").Consume())
            FireDotEvent(DotEventType.Left, modifiers);
        if (controls.Get<ButtonAction>("editor.field.caret.right").Consume())
            FireDotEvent(DotEventType.Right, modifiers);
        if (controls.Get<ButtonAction>("editor.field.caret.up").Consume()) FireDotEvent(DotEventType.Up, modifiers);
        if (controls.Get<ButtonAction>("editor.field.caret.down").Consume())
            FireDotEvent(DotEventType.Down, modifiers);
        if (controls.Get<ButtonAction>("editor.field.caret.home").Consume())
            FireDotEvent(DotEventType.Home, modifiers);
        if (controls.Get<ButtonAction>("editor.field.caret.end").Consume())
            FireDotEvent(DotEventType.End, modifiers);
        if (controls.Get<ButtonAction>("editor.field.select_all").Consume())
        {
            _dots.Clear();
            _dots.Add(new Dot(Document, 0, Document.Length));
            DotsUpdated();
            ResetBlinker();
        }
    }


    private void DotsUpdated()
    {
        // foreach(Dot dot in _dots) {
        // dot.Clamp();
        // }
    }

    private void ResetBlinker()
    {
        _blinkerTime = Time.Now;
    }

    private void FireDotEvent(DotEventType type, KeyModifiers modifiers)
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            Dot dot = _dots[i];
            dot.HandleEvent(type, modifiers);
            _dots[i] = dot;
        }

        DotsUpdated();
        ResetBlinker();
    }

    public void OnFocusGained()
    {
        ResetBlinker();
    }

    public void PreRender(IRenderingUnit renderer, IRenderingLayer layer, Point offset)
    {
        if (_parent.Focused)
        {
            int fieldWidth = _parent.Bounds.Width;
            fieldWidth -= 2 * (offset.X - _parent.Bounds.X);
            foreach (Dot dot in _dots) dot.DrawSelection(renderer, layer, offset, fieldWidth);
        }
    }

    public void PostRender(IRenderingUnit renderer, IRenderingLayer layer, Point offset)
    {
        if (_parent.Focused && BlinkerOn)
            foreach (Dot dot in _dots)
                dot.DrawIndex(renderer, layer, offset);
    }

    public CaretProfile CreateProfile()
    {
        var profile = new CaretProfile(_dots.ToArray());
        profile.Sort();
        return profile;
    }

    public void SetProfile(CaretProfile profile)
    {
        _dots.Clear();

        foreach (Dot dot in profile.Dots)
            if (dot.Document == Document)
                _dots.Add(dot);
            else
                _dots.Add(new Dot(Document, dot.Index, dot.Mark));
    }
}

public struct CaretProfile
{
    public readonly Dot[] Dots;

    public CaretProfile(CaretProfile other)
    {
        Dots = new Dot[other.Dots.Length];
        for (int i = 0; i < other.Dots.Length; i++) Dots[i] = other.Dots[i];
    }

    public CaretProfile(params Dot[] dots)
    {
        Dots = dots;
    }

    public void Sort()
    {
        Array.Sort(Dots, (a, b) => b.Index - a.Index);
    }
}