using System.Collections.Generic;
using FoldEngine.Commands;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;

namespace FoldEngine.Editor;

[GameSystem("fold:editor.base", ProcessingCycles.All, true)]
public class EditorBase : GameSystem
{
    public List<long> EditingEntity = new List<long>();
    // public override bool ShouldSave => false;

    [DoNotSerialize] public EditorEnvironment Environment;

    public override void SubscribeToEvents()
    {
        Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => { Environment.LayoutValidated = false; });
    }

    public override void Initialize()
    {
        Environment = new EditorEnvironment(Scene);

        Environment.AddView<EditorToolbarView>(Environment.NorthPanel);
        Environment.AddView<EditorHierarchyView>(Environment.WestPanel);
        Environment.AddView<EditorSystemsView>(Environment.WestPanel);
        Environment.AddView<EditorInspectorView>(Environment.EastPanel);
        Environment.AddView<EditorGameView>(Environment.CenterPanel);
        Environment.AddView<EditorResourcesView>(Environment.SouthPanel);
        Environment.AddView<EditorDebugActionsView>(Environment.EastPanel);
        // Environment.AddView<EditorTestView>(Environment.SouthPanel);

        Environment.WestPanel.ViewLists[0].ActiveView = Environment.GetView<EditorHierarchyView>();
        Environment.NorthPanel.ViewLists[0].ActiveView = Environment.GetView<EditorToolbarView>();
    }

    public override void OnInput()
    {
        Environment.Input(Scene.Core.InputUnit);
    }

    public override void OnUpdate()
    {
        Environment.Update();
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        if (!renderer.Groups.ContainsKey("editor")) return;
        if (renderer.RootGroup != renderer.Groups["editor"])
        {
            Scene.Core.CommandQueue.Enqueue(new SetRootRendererGroupCommand(renderer.Groups["editor"]));
            return;
        }

        Environment.Render(renderer, renderer.RootGroup["editor_gui"], renderer.RootGroup["editor_gui_overlay"]);

        foreach (long entityId in EditingEntity)
            if (Scene.Components.HasComponent<Transform>(entityId))
            {
                var entity = new Entity(Scene, entityId);

                LevelRenderer2D.DrawOutline(entity);
                ColliderGizmoRenderer.DrawColliderGizmos(entity);
            }
    }
}