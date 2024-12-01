using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools;

public class HandTool : EditorTool
{
    private bool _dragging;
    private Vector2 _dragStartWorldPos;

    public HandTool(EditorEnvironment environment) : base(environment)
    {
        Icon = EditorIcons.Hand;
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (cameraTransform.IsNull) return;

        IRenderingLayer worldLayer = Core.RenderingUnit.WorldLayer;
        Vector2 cameraPos = worldLayer.LayerToCamera(worldLayer.WindowToLayer(e.Position.ToVector2()));
        Vector2 worldPos = cameraTransform.Apply(cameraPos);

        _dragStartWorldPos = worldPos;
        _dragging = true;
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        _dragging = false;
    }

    public override void OnInput(ControlScheme controls)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (cameraTransform.IsNull) return;

        if (_dragging)
        {
            IRenderingLayer worldLayer = Core.RenderingUnit.WorldLayer;
            Vector2 cameraRelativePos =
                worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));

            cameraTransform.Position = _dragStartWorldPos;
            cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
        }
    }
}