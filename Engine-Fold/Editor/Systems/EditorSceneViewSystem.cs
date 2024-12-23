using System;
using FoldEngine.Components;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems;

[GameSystem("fold:editor.scene_view", ProcessingCycles.Render)]
public class EditorSceneViewSystem : GameSystem
{
    private ComponentIterator<EditorSceneViewPanel> _sceneViewPanels;

    public override void Initialize()
    {
        _sceneViewPanels = CreateComponentIterator<EditorSceneViewPanel>(IterationFlags.IncludeInactive);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        _sceneViewPanels.Reset();
        while (_sceneViewPanels.Next())
        {
            if (!_sceneViewPanels.HasCoComponent<Control>()) return;
            ref var hierarchical = ref _sceneViewPanels.GetCoComponent<Hierarchical>();
            ref var transform = ref _sceneViewPanels.GetCoComponent<Transform>();
            ref var control = ref _sceneViewPanels.GetCoComponent<Control>();

            var panelBounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());

            var renderDependency = renderer.Groups["editor"].Dependencies[0];
            if (hierarchical.IsActiveInHierarchy())
            {
                renderDependency.Group.Size = panelBounds.Size;
                renderDependency.Destination = panelBounds;
            }
            else
            {
                renderDependency.Group.Size = default;
            }
        }
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref MouseScrolledEvent evt) =>
        {
            if (!Scene.Components.HasComponent<EditorSceneViewPanel>(evt.EntityId)) return;
            
            var editorBase = Scene.Systems.Get<EditorBase>();
            
            ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
            if (cameraTransform.IsNull) return;

            IRenderingLayer worldLayer = Scene.Core.RenderingUnit.WorldLayer;
            Vector2 cameraRelativePos =
                worldLayer.LayerToCamera(worldLayer.WindowToLayer(evt.Position.ToVector2()));
            Vector2 pivot = cameraTransform.Apply(cameraRelativePos);

            cameraTransform.LocalScale -= cameraTransform.LocalScale * 0.1f * -evt.Amount;

            cameraTransform.Position = pivot;
            cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
        });
        
        
        Subscribe((ref MouseButtonEvent evt) =>
        {
            if (!Scene.Components.HasComponent<EditorSceneViewPanel>(evt.EntityId)) return;
            
            var toolSystem = Scene.Systems.Get<EditorToolSystem>();
            if (toolSystem != null)
            {
                if (evt.Button is MouseButtonEvent.MiddleButton or MouseButtonEvent.RightButton)
                {
                    toolSystem.ForcedTool = evt.Type == MouseButtonEventType.Pressed ? toolSystem.Tools[0] : null;
                }

                var evtTransformed = new MouseEvent()
                {
                    Position = evt.Position,
                    Button = evt.Button,
                    When = 0,
                    Consumed = false
                };
                
                if (evt.Type == MouseButtonEventType.Pressed)
                {
                    evtTransformed.Type = MouseEventType.Pressed;
                    toolSystem.ActiveTool?.OnMousePressed(ref evtTransformed);
                } else if (evt.Type == MouseButtonEventType.Released)
                {
                    evtTransformed.Type = MouseEventType.Released;
                    toolSystem.ActiveTool?.OnMouseReleased(ref evtTransformed);
                }
            }
        });
        
        Subscribe((ref HandleInputsEvent evt) =>
        {
            if (!Scene.Components.HasComponent<EditorSceneViewPanel>(evt.EntityId)) return;
            
            var editorBase = Scene.Systems.Get<EditorBase>();
        
            ref Transform cameraTransform = ref editorBase.CurrentCameraTransform;
            if (cameraTransform.IsNull) return;

            var controls = Scene.Systems.Get<ImmediateGuiSystem>().ControlScheme;
        
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
        });
    }
}