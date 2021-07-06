using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui.Fields {
    public class Caret {
        private List<Dot> _dots = new List<Dot>();
        private TextField _parent;

        private DocumentModel Document => _parent.Document;
        
        private long _blinkerTime = 0;
        public bool BlinkerOn => _parent.Pressed(MouseEvent.LeftButton) ||  ((Time.Now - _blinkerTime) / 500) % 2 == 0;

        public Caret(TextField parent) {
            _parent = parent;
            
            _dots.Add(new Dot(Document));
        }

        public int Dot {
            get => _dots[0].Index;
            set {
                Dot newDot = _dots[0];
                newDot.Index = newDot.Mark = value;
                _dots[0] = newDot;
            }
        }

        public int DotIndex {
            get => _dots[0].Index;
            set {
                Dot newDot = _dots[0];
                newDot.Index = value;
                _dots[0] = newDot;
            }
        }

        public int DotMark {
            get => _dots[0].Mark;
            set {
                Dot newDot = _dots[0];
                newDot.Mark = value;
                _dots[0] = newDot;
            }
        }

        public void OnInput(ControlScheme controls) {
            KeyModifiers modifiers = KeyModifiersExt.GetKeyModifiers();
            if(controls.Get<ButtonAction>("editor.field.caret.left").Consume()) {
                FireDotEvent(DotEventType.Left, modifiers);
            }
            if(controls.Get<ButtonAction>("editor.field.caret.right").Consume()) {
                FireDotEvent(DotEventType.Right, modifiers);
            }
            if(controls.Get<ButtonAction>("editor.field.caret.up").Consume()) {
                FireDotEvent(DotEventType.Up, modifiers);
            }
            if(controls.Get<ButtonAction>("editor.field.caret.down").Consume()) {
                FireDotEvent(DotEventType.Down, modifiers);
            }
            if(controls.Get<ButtonAction>("editor.field.caret.home").Consume()) {
                FireDotEvent(DotEventType.Home, modifiers);
            }
            if(controls.Get<ButtonAction>("editor.field.caret.end").Consume()) {
                FireDotEvent(DotEventType.End, modifiers);
            }
        }


        private void DotsUpdated() {
            // foreach(Dot dot in _dots) {
                // dot.Clamp();
            // }
        }

        private void ResetBlinker() {
            _blinkerTime = Time.Now;
        }

        private void FireDotEvent(DotEventType type, KeyModifiers modifiers) {
            for(int i = 0; i < _dots.Count; i++) {
                Dot dot = _dots[i];
                dot.HandleEvent(type, modifiers);
                _dots[i] = dot;
            }
            DotsUpdated();
            ResetBlinker();
        }

        public void OnFocusGained() {
            ResetBlinker();
        }

        public void PreRender(IRenderingUnit renderer, IRenderingLayer layer, Point offset) {
            int fieldWidth = _parent.Bounds.Width;
            fieldWidth -= 2 * (offset.X - _parent.Bounds.X);
            foreach(Dot dot in _dots) {
                dot.DrawSelection(renderer, layer, offset, fieldWidth);
            }
        }

        public void PostRender(IRenderingUnit renderer, IRenderingLayer layer, Point offset) {
            if(_parent.Focused && BlinkerOn) {
                foreach(Dot dot in _dots) {
                    dot.DrawIndex(renderer, layer, offset);
                }
            }
        }
    }
}