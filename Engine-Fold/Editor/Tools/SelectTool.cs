using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
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
        ref Transform cameraTransform = ref EditingScene.MainCameraTransform;

        IRenderingLayer worldLayer = Environment.Core.RenderingUnit.WorldLayer;
        Vector2 cameraRelativePos =
            worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));
        Vector2 worldPos = cameraTransform.Apply(cameraRelativePos);

        long intersectingEntities =
            EditingScene.Systems.Get<LevelRenderer2D>()?.ListEntitiesIntersectingPosition(worldPos) ?? -1;

        var editorBase = Scene.Systems.Get<EditorBase>();
        if (!Core.InputUnit.Devices.Keyboard[Keys.LeftControl].Down
            && !Core.InputUnit.Devices.Keyboard[Keys.RightControl].Down)
            editorBase.EditingEntity.Clear();

        if (intersectingEntities != -1 && !editorBase.EditingEntity.Contains(intersectingEntities))
            editorBase.EditingEntity.Add(intersectingEntities);
        Environment.SwitchToView<EditorInspectorView>();
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
    }
}