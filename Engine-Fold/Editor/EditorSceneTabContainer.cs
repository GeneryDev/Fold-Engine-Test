using FoldEngine.Components;

namespace FoldEngine.Editor;

[Component("fold:editor.scene_tab_container")]
[ComponentInitializer(typeof(EditorSceneTabContainer))]
public struct EditorSceneTabContainer
{
    public long TabListId = -1;

    public EditorSceneTabContainer()
    {
    }
}