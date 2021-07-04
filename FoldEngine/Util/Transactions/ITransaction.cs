namespace FoldEngine.Util.Transactions {
    public interface ITransaction { }

    public abstract class Transaction<T> : ITransaction {
        public readonly long Time = FoldEngine.Time.Now;

        public abstract bool Redo(T target);
        public abstract bool Undo(T target);

        public bool RedoOnInsert(T target) {
            return Redo(target);
        }
    }
}