using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Systems;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorSceneView : EditorView
{
    public EditorSceneView()
    {
        new ResourceIdentifier("editor/play");
    }

    public virtual string Name => "Scene";

    public override bool UseMargin => false;
    public override Color? BackgroundColor => Color.Black;

    public override void Render(IRenderingUnit renderer)
    {
        renderer.Groups["editor"].Dependencies[0].Group.Size = ContentPanel.Bounds.Size;
        renderer.Groups["editor"].Dependencies[0].Destination = ContentPanel.Bounds;

        var toolSystem = Scene.Systems.Get<EditorToolSystem>();
        toolSystem?.ActiveTool?.Render(renderer);
    }

    public override void EnsurePanelExists(GuiEnvironment environment)
    {
        if (ContentPanel == null) ContentPanel = new SceneViewPanel(environment);
    }
}

public class SceneViewPanel : GuiPanel
{
    public SceneViewPanel(GuiEnvironment environment) : base(environment)
    {
        MayScroll = true;
    }

    protected override bool Focusable => true;

    public override void OnMousePressed(ref MouseEvent e)
    {
        var toolSystem = Scene.Systems.Get<EditorToolSystem>();
        if (toolSystem != null)
        {
            if (e.Button is MouseEvent.MiddleButton or MouseEvent.RightButton)
                toolSystem.ForcedTool = toolSystem.Tools[0];

            toolSystem.ActiveTool?.OnMousePressed(ref e);
        }
        
        base.OnMousePressed(ref e);
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        var toolSystem = Scene.Systems.Get<EditorToolSystem>();
        if (toolSystem != null)
        {
            if (e.Button is MouseEvent.MiddleButton or MouseEvent.RightButton)
                toolSystem.ForcedTool = null;

            toolSystem.ActiveTool?.OnMouseReleased(ref e);
        }

        base.OnMouseReleased(ref e);
    }

    public override void Scroll(int dir)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (cameraTransform.IsNull) return;

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
        var editorBase = Scene.Systems.Get<EditorBase>();
        
        ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
        if (cameraTransform.IsNull) return;
        
        Vector2 move = default;
        if (controls != null)
        {
            move = controls.Get<AnalogAction>("editor.movement.axis.x") * Vector2.UnitX
                   + controls.Get<AnalogAction>("editor.movement.axis.y") * Vector2.UnitY;
        }

        if (move != default)
        {
            float speed = 250f;
            if (controls?.Get<ButtonAction>("editor.movement.faster").Down ?? false) speed *= 4;

            speed *= cameraTransform.LocalScale.X;

            cameraTransform.Position += move * speed * Time.DeltaTime;
        }

        
        var toolSystem = Scene.Systems.Get<EditorToolSystem>();
        toolSystem?.ActiveTool?.OnInput();
    }
}