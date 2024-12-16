using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Panels;

public static partial class EditorPanels
{
    private static Entity CreateEditorPanel(Scene scene, string name, string icon, long tabBarId, long tabContainerId)
    {
        var view = scene.CreateEntity(name);
        view.AddComponent<Control>().ZOrder = 0;
        view.AddComponent<AnchoredControl>() = new()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };
        view.Hierarchical.SetParent(tabContainerId);
            
        CreateTab(scene, name, icon, view.EntityId).Hierarchical.SetParent(tabBarId);
        return view;
    }
    private static Entity CreateTab(Scene scene, string name, string icon, long linkedControl)
    {
        var tab = scene.CreateEntity("Tab");
        tab.AddComponent<Control>().MinimumSize = new Vector2(0, 14);
        tab.AddComponent<ButtonControl>() = new ButtonControl
        {
            Text = name,
            Alignment = Alignment.Begin,
            Icon = new ResourceIdentifier(icon),
            Style = new ResourceIdentifier("editor:tab"),
            KeepPressedOutside = true
        };
        tab.AddComponent<Tab>() = new Tab()
        {
            DeselectedButtonStyle = "editor:tab",
            SelectedButtonStyle = "editor:tab.selected",
            LinkedEntityId = linkedControl
        };
        tab.AddComponent<EditorTab>() = new EditorTab()
        {
            TabName = name
        };
        return tab;
    }
}