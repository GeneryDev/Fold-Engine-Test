using FoldEngine.Editor.Components;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Panels;

public static partial class EditorPanels
{
    public static Entity SceneView(Scene scene, long tabBarId, long tabContainerId)
    {
        var panel = CreateEditorPanel(scene, "Scenes", "editor/play", tabBarId, tabContainerId);

        var sceneTabContainer = scene.CreateEntity("Open Scenes");
        sceneTabContainer.Hierarchical.SetParent(panel);

        var tabList = scene.CreateEntity("Scene Tab List");
        tabList.Hierarchical.SetParent(panel);
        tabList.AddComponent<Control>();
        tabList.AddComponent<TabList>();
        tabList.SetComponent(new StackContainer()
        {
            Alignment = Alignment.Begin,
            Separation = 1,
            Vertical = false
        });
        tabList.SetComponent(new TabSwitcher()
        {
            ContainerEntityId = sceneTabContainer.EntityId
        });

        var sceneView = scene.CreateEntity("Scene View");
        sceneView.Hierarchical.SetParent(panel);
        sceneView.AddComponent<Control>();
        sceneView.SetComponent(new BoxControl()
        {
            Color = Color.Black
        });
        sceneView.AddComponent<EditorSceneViewPanel>();
        
        sceneTabContainer.SetComponent(new EditorSceneTabContainer()
        {
            TabListId = tabList.EntityId
        });

        panel.SetComponent(new BorderContainer()
        {
            NorthPanelId = tabList.EntityId
        });

        return panel;
    }
}