using System;
using System.Collections.Generic;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui.Fields {
    public class Document {
        
        protected readonly List<char> Buffer = new List<char>();

        public bool Dirty { get; private set; } = true;
        
        private DocumentNode[] _nodes = new DocumentNode[16];
        private List<DocumentPage> _pages = new List<DocumentPage>();
        private int _nodeCount = 1;
        
        private int _currentIndex = 0;
        private int _currentColumn = 0;
        private int _currentLogicalLine = 0;
        private int _currentGraphicalLine = 0;
        private int _currentX = 0;
        private int _currentY = 0;

        public int Length => Buffer.Count;
        public int LogicalLines => _currentLogicalLine + 1;
        public int GraphicalLines => _currentGraphicalLine + 1;


        #region Model Construction

        private void EnsureSize(int size) {
            if(_nodes.Length < size) Array.Resize(ref _nodes, MathUtil.NearestPowerOfTwo(size));
        }

        private void ResetModel() {
            _currentIndex = _currentColumn = _currentLogicalLine = _currentGraphicalLine = _currentX = _currentY = 0;
            _pages.Clear();
            _nodeCount = 0;
            WritePage();
        }

        private Document WriteChar(int width) {
            EnsureSize(_nodeCount+1);
            _nodes[_nodeCount].SetChar(width);
            _nodeCount++;

            _currentIndex++;
            _currentColumn++;
            _currentX += width;
            
            return this;
        }

        private Document WriteBreak(bool logical) {
            EnsureSize(_nodeCount+1);
            _nodes[_nodeCount].SetBreak(logical);
            _nodeCount++;

            if(logical) {
                _currentLogicalLine++;
                
                _currentIndex++; // this is a \n so we count it
            }
            
            _currentGraphicalLine++;
            _currentColumn = 0;
            _currentX = 0;
            _currentY += 12; //TODO use line height or whatever

            if(logical) {
                WritePage();
            }
            
            return this;
        }

        private Document WritePage() {
            var page = new DocumentPage() {
                Index = _currentIndex,
                Column = _currentColumn,
                LogicalLine = _currentLogicalLine,
                GraphicalLine = _currentGraphicalLine,
                X = _currentX,
                Y = _currentY,
                NodeIndex = _nodeCount,
                PageIndex = _pages.Count
            };
            _pages.Add(page);

            return this;
        }

        private Document WriteEnd() {
            EnsureSize(_nodeCount+1);
            _nodes[_nodeCount].SetEnd();
            _nodeCount++;
            return this;
        }

        public void RebuildModel(TextRenderer textRenderer) {
            ResetModel();
            for(int i = 0; i <= Buffer.Count; i++) {
                if(i < Buffer.Count) {
                    char c = Buffer[i];
                    int prevX = textRenderer.Cursor.X;
                    textRenderer.Append(c);
                    if(c == '\n') {
                        WriteBreak(true);
                    } else {
                        WriteChar(textRenderer.Cursor.X - prevX);
                    }
                }
            }
            WriteEnd();
            Dirty = false;
        }

        #endregion

        #region Model To View Methods

        public int GetLogicalLineForIndex(int index) {
            DocumentPage startPage = GetStartPage(index);
            int value = startPage.LogicalLine;
            int currentIndex = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(currentIndex >= index) return value;
                
                if(node.IsBreak && node.BreakIsLogical) {
                    value++;
                }

                if(node.IsChar) {
                    currentIndex++;
                }

                if(node.IsEnd) return value;
            }

            return -1;
        }

        public int GetGraphicalLineForIndex(int index) {
            DocumentPage startPage = GetStartPage(index);
            int value = startPage.GraphicalLine;
            int currentIndex = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(currentIndex >= index) return value;
                
                if(node.IsBreak) {
                    value++;
                }

                if(node.IsChar) {
                    currentIndex++;
                }

                if(node.IsEnd) return value;
            }

            return -1;
        }

        public int GetXForIndex(int index) {
            DocumentPage startPage = GetStartPage(index);
            int value = startPage.X;
            int currentIndex = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(currentIndex >= index) return value;
                
                if(node.IsBreak) {
                    if(node.BreakIsLogical) currentIndex++;
                    value = 0;
                }

                if(node.IsChar) {
                    currentIndex++;
                    value += node.CharWidth;
                }

                if(node.IsEnd) return value;
            }

            return -1;
        }

        public int GetYForIndex(int index) {
            DocumentPage startPage = GetStartPage(index);
            int value = startPage.Y;
            int currentIndex = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(currentIndex >= index) return value;
                
                if(node.IsBreak) {
                    value += 12; //TODO line height
                }

                if(node.IsChar) {
                    currentIndex++;
                }

                if(node.IsEnd) return value;
            }

            return -1;
        }
        
        public Rectangle ModelToView(int index) {
            DocumentPage startPage = GetStartPage(index);
            Rectangle value = new Rectangle(startPage.X, startPage.Y - 12, 1, 12);
            int currentIndex = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(currentIndex >= index) {
                    if(node.IsChar) {
                        value.Width = node.CharWidth;
                    }
                    return value;
                }
                
                if(node.IsBreak) {
                    if(node.BreakIsLogical) currentIndex++;
                    value.X = 0;
                    value.Y += 12; //TODO line height
                }

                if(node.IsChar) {
                    currentIndex++;
                    value.X += node.CharWidth;
                }

                if(node.IsEnd) return value;
            }

            return default;
        }

        #endregion
        
        #region View To Model Methods
        
        public int ViewToModel(Point p) {
            DocumentPage startPage = _pages[_pages.Count-1];
            foreach(DocumentPage page in _pages) {
                if(p.Y <= page.Y) {
                    startPage = page;
                    break;
                }
            }

            int charX = startPage.X;
            int value = startPage.Index;
            for(int i = startPage.NodeIndex; i < _nodeCount; i++) {
                DocumentNode node = _nodes[i];

                if(charX >= p.X || node.IsBreak) return value;
                
                if(node.IsChar) {
                    value++;
                    charX += node.CharWidth;
                }

                if(charX >= p.X || node.IsEnd) return value;
            }

            return startPage.Index;
        }
        
        #endregion

        #region Model To Model Methods
        
        private DocumentPage GetStartPage(int index) {
            if(_pages.Count == 1) return _pages[0];
            for(int i = 1; i < _pages.Count; i++) {
                if(_pages[i].Index > index) {
                    return _pages[i-1];
                }
            }
            return _pages[_pages.Count-1];
        }

        public int GetRowStart(int index) {
            return GetStartPage(index).Index;
        }

        public int GetRowEnd(int index) {
            DocumentPage startPage = GetStartPage(index);
            if(startPage.PageIndex+1 >= _pages.Count) return Length;
            return _pages[startPage.PageIndex + 1].Index-1;
        }

        public int GetNextNonWhitespace(int index) {
            while(index < Length && char.IsWhiteSpace(Buffer[index])) {
                index++;
            }
            return index;
        }

        public int GetPositionAbove(int index, int x) {
            DocumentPage startPage = GetStartPage(index);
            if(startPage.PageIndex <= 0) return 0;
            return _pages[startPage.PageIndex - 1].GetIndexForX(x, _nodes, _nodeCount);
        }

        public int GetPositionBelow(int index, int x) {
            DocumentPage startPage = GetStartPage(index);
            if(startPage.PageIndex >= _pages.Count-1) return Length;
            return _pages[startPage.PageIndex + 1].GetIndexForX(x, _nodes, _nodeCount);
        }

        public int GetPreviousWord(int index) {
            return index - 1; //TODO
        }

        public int GetNextWord(int index) {
            return index + 1; //TODO
        }

        public int GetWordStart(int index) {
            return index - 1; //TODO
        }

        public int GetWordEnd(int index) {
            return index + 1; //TODO
        }

        #endregion

        #region Buffer Modification

        public string Text {
            get => new string(Buffer.ToArray());
            set {
                Buffer.Clear();
                if(Buffer.Capacity < value.Length) Buffer.Capacity = value.Length;
                foreach(char c in value) {
                    Buffer.Add(c);
                }

                Dirty = true;
            }
        }

        public void Insert(int start, IEnumerable<char> chars) {
            Replace(start, 0, chars);
        }

        public void Remove(int start, int length) {
            Replace(start, length, null);
        }

        public void Replace(int start, int length, IEnumerable<char> chars) {
            if(length == 0 && chars == null) return;

            if(length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if(start + length > Length) throw new ArgumentOutOfRangeException(nameof(start));
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));

            for(int i = start; i < start + length; i++) {
                Buffer.RemoveAt(start);
            }
            if(chars != null) {
                Buffer.InsertRange(start, chars);
            }

            Dirty = true;
        }
        
        

        public void Insert(int start, char c) {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));

            Buffer.Insert(start, c);
            
            Dirty = true;
        }

        #endregion
    }

    public struct DocumentNode {
        private byte _type;
        private int _value;

        public bool IsEnd => _type == 0;
        public bool IsChar => _type == 1;
        public bool IsBreak => _type == 2;
        
        public void SetChar(int width) {
            _type = 1;
            _value = width;
        }

        public void SetBreak(bool logical) {
            _type = 2;
            _value = logical ? 1 : 0;
        }

        public void SetEnd() {
            _type = 0;
        }

        public int CharWidth {
            get {
                if(!IsChar) throw new InvalidOperationException("Not a char");
                return _value;
            }
        }

        public bool BreakIsLogical {
            get {
                if(!IsBreak) throw new InvalidOperationException("Not a break");
                return _value == 1;
            }
        }
    }

    public struct DocumentPage {
        public int Index;
        public int Column;
        public int LogicalLine;
        public int GraphicalLine;
        
        public int X;
        public int Y;
        
        public int NodeIndex;
        public int PageIndex;

        public int GetIndexForX(int x, DocumentNode[] nodes, int nodeCount) {
            int currentX = X;
            int currentIndex = Index;
            for(int i = NodeIndex; i < nodeCount; i++) {
                DocumentNode node = nodes[i];

                if(currentX >= x || node.IsBreak) return currentIndex;
                
                if(node.IsChar) {
                    currentIndex++;
                    currentX += node.CharWidth;
                }

                if(currentX >= x || node.IsEnd) return currentIndex;
            }

            return -1;
        }
    }
}