using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor;

[Component("fold:editor.tab")]
[ComponentInitializer(typeof(EditorTab), nameof(InitializeComponent))]
public struct EditorTab
{
    [DoNotSerialize]
    public TransactionManager<Scene> SceneTransactions;
    public List<long> EditingEntity;
    [EntityId] public long EditorCameraEntityId; 
    public bool PreviewSceneCamera;

    public EditorTab()
    {
        EditingEntity = new List<long>();
        EditorCameraEntityId = -1;
    }
    
    /// <summary>
    ///     Returns an initialized editor tab component with all its correct default values.
    /// </summary>
    public static EditorTab InitializeComponent(Scene scene, long entityId)
    {
        return new EditorTab();
    }
}