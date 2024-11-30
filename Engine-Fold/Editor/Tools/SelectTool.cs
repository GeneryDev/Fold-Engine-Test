using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Views;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Tools;

public class SelectTool : EditorTool
{
    public SelectTool(EditorEnvironment environment) : base(environment)
    {
        Icon = EditorIcons.Cursor;
    }

    public override void OnInput(ControlScheme controls)
    {
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        ref Transform cameraTransform = ref editingTab.Scene.MainCameraTransform;

        IRenderingLayer worldLayer = Environment.Core.RenderingUnit.WorldLayer;
        Vector2 cameraRelativePos =
            worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));
        Vector2 worldPos = cameraTransform.Apply(cameraRelativePos);

        long intersectingEntities =
            editingTab.Scene.Systems.Get<LevelRenderer2D>()?.ListEntitiesIntersectingPosition(worldPos) ?? -1;

        if (!Core.InputUnit.Devices.Keyboard[Keys.LeftControl].Down
            && !Core.InputUnit.Devices.Keyboard[Keys.RightControl].Down)
            editingTab.EditingEntity.Clear();

        if (intersectingEntities != -1 && !editingTab.EditingEntity.Contains(intersectingEntities))
            editingTab.EditingEntity.Add(intersectingEntities);
        Environment.SwitchToView<EditorInspectorView>();
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
    }
}