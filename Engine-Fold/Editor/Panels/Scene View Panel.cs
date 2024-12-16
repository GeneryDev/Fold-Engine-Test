using FoldEngine.Editor.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Panels;

public static partial class EditorPanels
{
    public static Entity SceneView(Scene scene, long tabBarId, long tabContainerId)
    {
        var view = CreateEditorPanel(scene, "Scene", "editor/play", tabBarId, tabContainerId);

        var sceneView = scene.CreateEntity("Scene View");
        sceneView.Hierarchical.SetParent(view);
        sceneView.AddComponent<Control>();
        sceneView.SetComponent(new BoxControl()
        {
            Color = Color.Black
        });
        sceneView.SetComponent(new AnchoredControl()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        });
        sceneView.AddComponent<EditorSceneViewPanel>();

        return view;
    }
}