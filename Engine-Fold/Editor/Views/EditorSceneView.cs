using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorSceneView : EditorView
{
    public EditorSceneView()
    {
        Icon = new ResourceIdentifier("editor/play");
    }

    public override string Name => "Scene";

    public override bool UseMargin => false;
    public override Color? BackgroundColor => Color.Black;

    public override void Render(IRenderingUnit renderer)
    {
        renderer.Groups["editor"].Dependencies[0].Group.Size = ContentPanel.Bounds.Size;
        renderer.Groups["editor"].Dependencies[0].Destination = ContentPanel.Bounds;

        ((EditorEnvironment)ContentPanel.Environment).ActiveTool?.Render(renderer);
    }

    public override void EnsurePanelExists(GuiEnvironment environment)
    {
        if (ContentPanel == null) ContentPanel = new SceneViewPanel(this, environment);
    }
}

public class SceneViewPanel : GuiPanel
{
    public SceneViewPanel(EditorSceneView editorSceneView, GuiEnvironment environment) : base(environment)
    {
        MayScroll = true;
    }

    public override bool Focusable => true;

    public override void OnMousePressed(ref MouseEvent e)
    {
        if (e.Button == MouseEvent.MiddleButton || e.Button == MouseEvent.RightButton)
            ((EditorEnvironment)Environment).ForcedTool = ((EditorEnvironment)Environment).Tools[0];

        ((EditorEnvironment)Environment).ActiveTool?.OnMousePressed(ref e);
        base.OnMousePressed(ref e);
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (e.Button == MouseEvent.MiddleButton || e.Button == MouseEvent.RightButton)
            ((EditorEnvironment)Environment).ForcedTool = null;

        base.OnMouseReleased(ref e);
        ((EditorEnvironment)Environment).ActiveTool?.OnMouseReleased(ref e);
    }

    public override void Scroll(int dir)
    {
        var editorBase = ((EditorEnvironment)Environment).EditorBase;
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (!cameraTransform.IsNotNull) return;

        IRenderingLayer worldLayer = Environment.Core.RenderingUnit.WorldLayer;
        Vector2 cameraRelativePos =
            worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));
        Vector2 pivot = cameraTransform.Apply(cameraRelativePos);

        cameraTransform.LocalScale -= cameraTransform.LocalScale * 0.1f * dir;

        cameraTransform.Position = pivot;
        cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
    }

    public override void OnInput(ControlScheme controls)
    {
        var editorBase = ((EditorEnvironment)Environment).EditorBase;
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (cameraTransform.IsNull) return;
        
        Vector2 move = controls.Get<AnalogAction>("editor.movement.axis.x") * Vector2.UnitX
                       + controls.Get<AnalogAction>("editor.movement.axis.y") * Vector2.UnitY;
        if (move != default)
        {
            float speed = 250f;
            if (controls.Get<ButtonAction>("editor.movement.faster").Down) speed *= 4;

            speed *= cameraTransform.LocalScale.X;

            cameraTransform.Position += move * speed * Time.DeltaTime;
        }

        ((EditorEnvironment)Environment).ActiveTool?.OnInput(controls);
    }
}