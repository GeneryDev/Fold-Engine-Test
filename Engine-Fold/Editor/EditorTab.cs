using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor;

[Component("fold:editor.tab")]
public struct EditorTab
{
    [DoNotSerialize]
    public TransactionManager<Scene> SceneTransactions;
    public List<long> EditingEntity;
}