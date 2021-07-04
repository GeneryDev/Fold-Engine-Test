using System;
using System.Collections.Generic;

namespace FoldEngine.Editor.Transactions {
    public class CompoundTransaction<T> : Transaction<T> {
        private readonly List<Lazy<Transaction<T>>> _edits;

        public int Count => _edits.Count;

        public CompoundTransaction() {
            _edits = new List<Lazy<Transaction<T>>>();
        }
        
        public CompoundTransaction(List<Lazy<Transaction<T>>> edits) {
            _edits = edits;
        }
        
        
        public void Append(Lazy<Transaction<T>> edit) {
            _edits.Add(edit);
        }

        public void Append(Func<Transaction<T>> edit) {
            Append(new Lazy<Transaction<T>>(edit));
        }

        public override bool Redo(T target) {
            bool actionPerformed = false;
            foreach(Lazy<Transaction<T>> e in _edits) {
                if(e.Value.Redo(target)) actionPerformed = true;
            }
            return actionPerformed;
        }

        public override bool Undo(T target) {
            bool actionPerformed = false;
            for(int i = _edits.Count-1; i >= 0; i--) {
                Lazy<Transaction<T>> e = _edits[i];
                if(e.Value.Undo(target)) actionPerformed = true;
            }
            return actionPerformed;
        }

        public bool redoOnInsert(T target) {
            bool actionPerformed = false;
            foreach(Lazy<Transaction<T>> e in _edits) {
                if(e.Value.RedoOnInsert(target)) actionPerformed = true;
            }
            return actionPerformed;
        }
    }
}