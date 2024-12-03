using FoldEngine.Components;
using FoldEngine.Editor.Components.Traits;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Components;

[Component("fold:editor.transaction_action", traits: [typeof(EditorAction)])]
public struct TransactionActionComponent
{
    public Transaction<Scene> Transaction;

    public TransactionActionComponent(Transaction<Scene> transaction)
    {
        Transaction = transaction;
    }
}