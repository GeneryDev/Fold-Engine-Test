using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Input;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Panels;

public static partial class EditorPanels
{
    public static Entity Toolbar(Scene scene, long tabBarId, long tabContainerId)
    {
        var view = CreateEditorPanel(scene, "Toolbar", "editor/cog", tabBarId, tabContainerId);

        view.AddComponent<FlowContainer>() = new FlowContainer()
        {
            HSeparation = 4,
            Alignment = Alignment.Begin
        };

        void AddToolbarButton(string name, string icon)
        {
            var btnEntity = scene.CreateEntity("Toolbar Button");
            btnEntity.AddComponent<Control>() = new Control()
            {
                MinimumSize = new Vector2(24, 24)
            };
            btnEntity.AddComponent<ButtonControl>() = new ButtonControl()
            {
                Icon = new ResourceIdentifier(icon),
                KeepPressedOutside = true
            };
            btnEntity.AddComponent<SimpleTooltip>() = new SimpleTooltip()
            {
                Text = name
            };
            btnEntity.AddComponent<PopupProvider>() = new PopupProvider()
            {
                ButtonMask = MouseButtonMask.RightButton,
                ActionMode = MouseActionMode.Release
            };
            btnEntity.Hierarchical.SetParent(view);
        }
        
        AddToolbarButton("Hand","editor/hand");
        AddToolbarButton("Move","editor/move");
        AddToolbarButton("Scale","editor/scale");
        AddToolbarButton("Rotate","editor/rotate");
        AddToolbarButton("Select","editor/cursor");
        AddToolbarButton("Play Scene","editor/play");
        AddToolbarButton("Pause Scene","editor/pause");

        return view;
    }
}