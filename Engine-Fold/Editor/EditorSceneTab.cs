using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor;

[Component("fold:editor.scene_tab")]
[ComponentInitializer(typeof(EditorSceneTab), nameof(InitializeComponent))]
public struct EditorSceneTab
{
    [DoNotSerialize] [HideInInspector] public Scene Scene;
    
    [DoNotSerialize]
    public TransactionManager<Scene> SceneTransactions;
    public List<long> EditingEntity;
    
    [EntityId] public long EditorCameraEntityId;
    public bool PreviewSceneCamera;
    
    [HideInInspector] public bool Playing;
    [HideInInspector] [DoNotSerialize] public byte[] StoredSceneData;

    public EditorSceneTab()
    {
        EditingEntity = new List<long>();
        EditorCameraEntityId = -1;
    }
    
    /// <summary>
    ///     Returns an initialized editor tab component with all its correct default values.
    /// </summary>
    public static EditorSceneTab InitializeComponent(Scene scene, long entityId)
    {
        return new EditorSceneTab();
    }
}