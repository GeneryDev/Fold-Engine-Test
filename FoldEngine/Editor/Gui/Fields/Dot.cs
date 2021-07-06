using System;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui.Fields {
    public struct Dot {
        private const bool SMART_KEYS_HOME = false;
        
        public Document Document;
        
        public int Index;
        public int Mark;
        public int X;
        
        public int Min => Math.Min(Index, Mark);
        public int Max => Math.Max(Index, Mark);
        
        public bool IsPoint => Index == Mark;

        public bool InIndentation => this.Index >= GetRowStart() && this.Index <= GetRowContentStart();


        public Dot(Document document) : this() {
            Document = document;
        }

        public Dot(Document document, int index) : this() {
            Document = document;
            Index = Mark = index;
        }

        public void UpdateX() {
            Rectangle view = Document.ModelToView(Index);
            X = view.X;
        }

        public bool HandleEvent(DotEventType type, KeyModifiers modifiers) {
            
            bool actionPerformed = false;
            int nextPos = 0;
            bool doUpdateX = false;

            switch(type) {
                case DotEventType.Left: {
                    if(!IsPoint && !modifiers.Has(KeyModifiers.Shift)) {
                        nextPos = Math.Min(Index, Mark);
                        if(modifiers.Has(KeyModifiers.Control)) nextPos = GetPositionBeforeWord();
                    } else nextPos = (modifiers.Has(KeyModifiers.Control)) ? GetPositionBeforeWord() : GetPositionBefore();
                    doUpdateX = true;
                    actionPerformed = true;
                    break;
                }
                case DotEventType.Right: {
                    if(!IsPoint && !modifiers.Has(KeyModifiers.Shift)) {
                        nextPos = Math.Max(Index, Mark);
                        if(modifiers.Has(KeyModifiers.Control)) nextPos = GetPositionAfterWord();
                    } else nextPos = (modifiers.Has(KeyModifiers.Control)) ? GetPositionAfterWord() : GetPositionAfter();
                    doUpdateX = true;
                    actionPerformed = true;
                    break;
                }
                case DotEventType.Up: {
                    if(modifiers.Has(KeyModifiers.Control)) {
                        return false;
                    }
                    nextPos = GetPositionAbove();
                    if(nextPos < 0) {
                        nextPos = 0;
                        doUpdateX = true;
                    }
                    actionPerformed = true;
                    break;
                }
                case DotEventType.Down: {
                    if(modifiers.Has(KeyModifiers.Control)) {
                        return false;
                    }
                    nextPos = GetPositionBelow();
                    if(nextPos < 0) {
                        nextPos = Document.Length;
                        doUpdateX = true;
                    }
                    actionPerformed = true;
                    break;
                }
                case DotEventType.Home: {
                    if (!modifiers.Has(KeyModifiers.Control)) {
                        nextPos = GetRowHome();
                    }
                    doUpdateX = true;
                    actionPerformed = true;
                    break;
                }
                case DotEventType.End: {
                    if(modifiers.Has(KeyModifiers.Control)) nextPos = Document.Length;
                    else nextPos = GetRowEnd();
                    doUpdateX = true;
                    actionPerformed = true;
                    break;
                }
            }
            if(actionPerformed) {
                Index = nextPos;
                if(!modifiers.Has(KeyModifiers.Shift)) Mark = nextPos;
                if(doUpdateX) UpdateX();
            }
            return actionPerformed;
        }

        public void Deselect() {
            Mark = Index;
        }

        public int GetPositionBefore() {
            return Math.Max(0, Math.Min(Index - 1, Document.Length));
        }

        public int GetPositionAfter() {
            return Math.Max(0, Math.Min(Index + 1, Document.Length));
        }

        public int GetPositionAbove() {
            return Document.GetPositionAbove(Index, X);
        }

        public int GetPositionBelow() {
            return Document.GetPositionBelow(Index, X);
        }

        public int GetPositionBeforeWord() {
            return Math.Max(0, Math.Max(Document.GetPreviousWord(Index), GetRowStart()-1));
        }

        public int GetPositionAfterWord() {
            int pos = Document.GetNextWord(Index);
            int rowEnd = GetRowEnd();
            return (Index == rowEnd) ? pos : Math.Min(pos, rowEnd);
        }

        public int GetRowHome() {
            int rowStart = GetRowStart();
            if(!SMART_KEYS_HOME) return rowStart;
            int rowContentStart = GetRowContentStart();
            if(Index == rowStart) return rowContentStart;
            if(Index <= rowContentStart) return rowStart;
            return rowContentStart;
        }

        public int GetWordStart() {
            return Math.Max(0, Math.Min(Document.GetWordStart(Index), GetRowStart()-1));
        }

        public int GetWordEnd() {
            int pos = Document.GetWordEnd(Index);
            int rowEnd = GetRowEnd();
            return (Index == rowEnd) ? pos : Math.Min(pos, rowEnd);
        }

        public int GetRowStart() {
            return Document.GetRowStart(Index);
        }

        public int GetRowEnd() {
            return Document.GetRowEnd(Index);
        }

        public int GetRowContentStart() {
            return Document.GetNextNonWhitespace(GetRowStart());
        }

        public bool Intersects(Dot other) {
            if(other.Min < this.Min) {
                return other.Intersects(this);
            }

            if(this.Mark == other.Mark && this.IsPoint != other.IsPoint) return false;
            return other.Min < this.Max;
        }

        public bool Contains(int index) {
            return Min <= index && index < Max;
        }

        public void Absorb(Dot other) {
            int newMin = Math.Min(this.Min, other.Min);
            int newMax = Math.Max(this.Max, other.Max);

            if(other.Mark <= newMin || this.Mark <= newMin) {
                this.Mark = newMin;
                this.Index = newMax;
            } else {
                this.Mark = newMax;
                this.Index = newMin;
            }
        }

        public void Clamp() {
            Index = Math.Max(0, Math.Min(Document.Length, Index));
            Mark = Math.Max(0, Math.Min(Document.Length, Mark));
        }

        public void DrawIndex(IRenderingUnit renderer, IRenderingLayer layer, Point offset) {
            Rectangle rect = Document.ModelToView(Index);
            rect.Offset(offset);
            rect.Width = 1;
            
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = Color.White,
                DestinationRectangle = rect
            });
        }

        public void DrawSelection(IRenderingUnit renderer, IRenderingLayer layer, Point offset, int maxWidth) {
            if(IsPoint) return;
            int startLine = Document.GetGraphicalLineForIndex(Min);
            int endLine = Document.GetGraphicalLineForIndex(Max);

            Rectangle startRect = Document.ModelToView(Min);
            Rectangle endRect = Document.ModelToView(Max);
            if(startLine == endLine) {
                //Single rectangle
                
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = Color.CornflowerBlue,
                    DestinationRectangle = new Rectangle(startRect.X + offset.X, startRect.Y + offset.Y, endRect.X - startRect.X, endRect.Height)
                });
            } else {
                
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = Color.CornflowerBlue,
                    DestinationRectangle = new Rectangle(startRect.X + offset.X, startRect.Y + offset.Y, maxWidth - startRect.X, startRect.Height)
                });
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = Color.CornflowerBlue,
                    DestinationRectangle = new Rectangle(offset.X, startRect.Y + startRect.Height + offset.Y, maxWidth, endRect.Y - startRect.Y - startRect.Height)
                });
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = Color.CornflowerBlue,
                    DestinationRectangle = new Rectangle(offset.X, endRect.Y + offset.Y, endRect.X, endRect.Height)
                });
            }
        }
    }

    public enum DotEventType {
        Left, Right, Up, Down, Home, End
    }
}