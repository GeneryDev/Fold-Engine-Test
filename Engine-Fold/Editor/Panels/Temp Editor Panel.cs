using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Panels;

public static partial class EditorPanels
{
    public static Entity CreateTempPanel(Scene scene, string name, string icon, long tabBarId, long tabContainerId)
    {
        var view = CreateEditorPanel(scene, name, icon, tabBarId, tabContainerId);

        view.AddComponent<FlowContainer>() = new FlowContainer()
        {
            HSeparation = 4,
            Alignment = Alignment.Center
        };

        var label = scene.CreateEntity("Temp Label");
        label.AddComponent<Control>();
        label.AddComponent<LabelControl>() = new LabelControl()
        {
            Text = name
        };
        label.Hierarchical.SetParent(view);

        return view;
    }
    public static Entity Immediate<T>(Scene scene, string name, string icon, long tabBarId, long tabContainerId) where T : EditorView, new()
    {
        var view = CreateEditorPanel(scene, name, icon, tabBarId, tabContainerId);

        var content = scene.CreateEntity("Immediate Content");
        content.AddComponent<Control>();
        content.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            Anchor = AnchoredControl.Presets.FullRect
        };
        content.AddComponent<ImmediateGuiControl>() = new ImmediateGuiControl()
        {
            View = new T()
        };
        content.Hierarchical.SetParent(view);

        return view;
    }
}