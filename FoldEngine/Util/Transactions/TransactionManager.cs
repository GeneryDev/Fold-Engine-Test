using System;
using System.Collections.Generic;

namespace FoldEngine.Util.Transactions {
    public class TransactionManager<T> {
        public delegate void TransactionListener(Transaction<T> transaction);

        private readonly List<Transaction<T>> _transactions = new List<Transaction<T>>();
        private readonly int _chainDelay = 100;
        private int _currentTransaction;

        protected T Target;

        public TransactionManager(T target) {
            Target = target;
        }

        private event TransactionListener OnTransactionInserted;
        private event TransactionListener OnTransactionUndone;
        private event TransactionListener OnTransactionRedone;

        public void Undo() {
            while(true) {
                if(_currentTransaction - 1 >= 0)
                    if(CanUndo()) {
                        Transaction<T> transaction = _transactions[--_currentTransaction];
                        transaction.Undo(Target);
                        OnTransactionUndone?.Invoke(_transactions[_currentTransaction]);
                        if(_currentTransaction > 0
                           && Math.Abs(_transactions[_currentTransaction].Time
                                       - _transactions[_currentTransaction - 1].Time)
                           <= _chainDelay) continue;
                    }

                break;
            }
        }

        public void Redo() {
            while(true) {
                if(_currentTransaction < _transactions.Count)
                    if(CanRedo()) {
                        Transaction<T> transaction = _transactions[_currentTransaction++];
                        transaction.Redo(Target);
                        OnTransactionRedone?.Invoke(_transactions[_currentTransaction]);
                        if(_currentTransaction < _transactions.Count
                           && Math.Abs(_transactions[_currentTransaction - 1].Time
                                       - _transactions[_currentTransaction].Time)
                           <= _chainDelay) continue;
                    }

                break;
            }
        }

        public void InsertTransaction(Transaction<T> transaction) {
            if(transaction.RedoOnInsert(Target)) {
                while(_transactions.Count > _currentTransaction) _transactions.RemoveAt(_currentTransaction);
                _transactions.Add(transaction);
                _currentTransaction++;
                OnTransactionInserted?.Invoke(_transactions[_currentTransaction]);
            }
        }

        protected virtual bool CanUndo() {
            return true;
        }

        protected virtual bool CanRedo() {
            return true;
        }

        public void Clear() {
            _transactions.Clear();
            _currentTransaction = 0;
        }
    }
}