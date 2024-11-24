using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Tools;

public class RotateTool : SelectTool
{
    private bool _dragging;

    private Transform _movePivot;
    private float _newRotation;
    private readonly List<Vector2> _pressEntityPivotPosition = new List<Vector2>();
    private readonly List<float> _pressEntityRotation = new List<float>();
    private Vector2 _pressMousePivotPosition;
    private float _pressMousePivotRotation;

    private float _startRotation;
    private readonly List<SetEntityTransformTransaction> _transactions = new List<SetEntityTransformTransaction>();

    private bool hoveringRing;

    public RotateTool(EditorEnvironment environment) : base(environment)
    {
        Icon = EditorIcons.Rotate;
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        if (hoveringRing)
        {
            var editorBase = Scene.Systems.Get<EditorBase>();
            var editingTab = editorBase.CurrentTab;
            if (editingTab.Scene == null) return;
            
            Vector2 mouseWorldPos =
                editingTab.Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                    Environment.Renderer.GizmoLayer.WindowToLayer(e.Position.ToVector2())));
            _pressMousePivotPosition = _movePivot.Relativize(mouseWorldPos);
            _pressMousePivotRotation = (float)Math.Atan2(_pressMousePivotPosition.Y, _pressMousePivotPosition.X);

            _pressEntityPivotPosition.Clear();
            _pressEntityRotation.Clear();
            _transactions.Clear();
            foreach (long entityId in editingTab.EditingEntity)
            {
                if (entityId == -1) continue;

                var entity = new Entity(editingTab.Scene, entityId);

                Vector2 relativeEntityPos = _movePivot.Relativize(entity.Transform.Position);

                _pressEntityPivotPosition.Add(relativeEntityPos);
                _pressEntityRotation.Add(entity.Transform.Rotation);

                var transaction = new SetEntityTransformTransaction(entity.Transform.CreateSnapshot());
                Environment.TransactionManager.InsertTransaction(transaction);
                _transactions.Add(transaction);
            }

            _dragging = true;
        }
        else
        {
            base.OnMousePressed(ref e);
        }
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        _dragging = false;
        _transactions.Clear();
        _pressEntityPivotPosition.Clear();
        base.OnMouseReleased(ref e);
    }

    private void EnsurePivotExists()
    {
        if (_movePivot.IsNull) _movePivot = Transform.InitializeComponent(Scene, 0);
    }

    private float SnapAngle(float angle)
    {
        float snap = (float)(22.5f * Math.PI / 180);
        return (float)(Math.Round(angle / snap) * snap);
    }

    public override void Render(IRenderingUnit renderer)
    {
        EnsurePivotExists();

        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentTab;
        if (editingTab.Scene == null) return;

        bool any = false;
        if (_dragging)
        {
            Vector2 mouseWorldPos =
                editingTab.Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                    Environment.Renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2())));

            Vector2 mouseOffset = _movePivot.Relativize(mouseWorldPos);
            float newRotation = (float)Math.Atan2(mouseOffset.Y, mouseOffset.X);

            if (Core.InputUnit.Devices.Keyboard[Keys.LeftShift].Down
                || Core.InputUnit.Devices.Keyboard[Keys.RightShift].Down)
                newRotation = SnapAngle(newRotation - _pressMousePivotRotation + _startRotation)
                              + _pressMousePivotRotation
                              - _startRotation;

            _movePivot.LocalRotation = newRotation - _pressMousePivotRotation + _startRotation;
            int i = 0;
            foreach (long entityId in editingTab.EditingEntity)
            {
                if (entityId == -1) continue;
                any = true;

                var entity = new Entity(editingTab.Scene, entityId);

                entity.Transform.Position = _movePivot.Position;
                entity.Transform.Position = _movePivot.Apply(_pressEntityPivotPosition[i]);

                entity.Transform.Rotation = _pressEntityRotation[i] + (newRotation - _pressMousePivotRotation);

                _transactions[i].UpdateAfter(entity.Transform.CreateSnapshot());

                i++;
            }

            _newRotation = newRotation - _pressMousePivotRotation + _startRotation;

            _movePivot.LocalRotation = _startRotation;
        }
        else
        {
            _movePivot.LocalPosition = default;
            foreach (long entityId in editingTab.EditingEntity)
            {
                if (entityId == -1) continue;
                any = true;

                var entity = new Entity(editingTab.Scene, entityId);

                _movePivot.LocalPosition += entity.Transform.Position;
                _startRotation = _movePivot.Rotation = entity.Transform.Rotation;
            }

            if (any) _movePivot.LocalPosition /= editingTab.EditingEntity.Count;
        }

        if (any)
        {
            Vector2 origin = _movePivot.LocalPosition;
            Complex rotationA = (_movePivot.Apply(Vector2.UnitX) - origin).Normalized();

            RenderLine(renderer,
                origin,
                origin + (Vector2)((Complex)Vector2.UnitX * rotationA),
                Color.Blue,
                editingTab.Scene.MainCameraTransform,
                120
            );

            if (_dragging)
            {
                _movePivot.LocalRotation = _newRotation;

                Complex rotationB = (_movePivot.Apply(Vector2.UnitX) - origin).Normalized();

                RenderLine(renderer,
                    origin,
                    origin + (Vector2)((Complex)Vector2.UnitX * rotationB),
                    new Color(200,
                        200,
                        255),
                    editingTab.Scene.MainCameraTransform,
                    120
                );
                _movePivot.LocalRotation = _startRotation;
            }

            RenderRing(renderer,
                origin,
                origin + (Vector2)((Complex)Vector2.UnitX * rotationA),
                Color.Blue,
                new Color(200,
                    200,
                    255),
                0, (float)(Math.PI / 2),
                out hoveringRing,
                _dragging ? hoveringRing : (bool?)null,
                    editingTab.Scene.MainCameraTransform,
                120);
        }
    }

    private void RenderLine(
        IRenderingUnit renderer,
        Vector2 start,
        Vector2 end,
        Color color,
        Transform cameraTransform,
        float fixedLength = 0)
    {
        start = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(start));
        end = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(end));

        if (fixedLength > 0) end = (end - start).Normalized() * fixedLength + start;

        Vector2 dir = (end - start).Normalized();
        Complex dirComplex = dir;

        float thickness = 2;

        //Render line

        renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
        {
            A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            Texture = renderer.WhiteTexture,
            Color = color
        });
        renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
        {
            A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            Texture = renderer.WhiteTexture,
            Color = color
        });
    }

    private void RenderRing(
        IRenderingUnit renderer,
        Vector2 start,
        Vector2 end,
        Color defaultColor,
        Color hoverColor,
        float startRotation,
        float endRotation,
        out bool hovered,
        bool? forceHoverState,
        Transform cameraTransform,
        float fixedRadius = 0)
    {
        start = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(start));
        end = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(end));

        if (fixedRadius > 0) end = (end - start).Normalized() * fixedRadius + start;


        Vector2 dir = (end - start).Normalized();
        Complex dirComplex = dir;

        float thickness = 2;
        float hoverDistance = 16;

        //Check hover
        if (!forceHoverState.HasValue)
        {
            Vector2 mousePosLayerSpace = renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2());
            float distance = Math.Abs(Vector2.Distance(start, mousePosLayerSpace)
                                      - Vector2.Distance(start, end));
            hovered = distance <= hoverDistance;
        }
        else
        {
            hovered = forceHoverState.Value;
        }
        //
        // //Render line
        //
        // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
        //     A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
        //     B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
        //     C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
        //     Texture = renderer.WhiteTexture,
        //     Color = hovered ? hoverColor : defaultColor
        // });
        // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
        //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
        //     B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
        //     C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
        //     Texture = renderer.WhiteTexture,
        //     Color = hovered ? hoverColor : defaultColor
        // });
        //
        // // Render head
        //
        // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
        //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
        //     B = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
        //     C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
        //     Texture = renderer.WhiteTexture,
        //     Color = hovered ? hoverColor : defaultColor
        // });
        // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
        //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
        //     B = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
        //     C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
        //     Texture = renderer.WhiteTexture,
        //     Color = hovered ? hoverColor : defaultColor
        // });


        int segments = 24;

        Vector2 endInner = end - dir * thickness / 2;
        Vector2 endOuter = end + dir * thickness / 2;

        Complex delta = Complex.FromRotation((float)(Math.PI * (360f / segments) / 180));
        for (int i = 0; i < segments; i++)
        {
            Vector2 nextInner = (Vector2)((Complex)(endInner - start) * delta) + start;
            Vector2 nextOuter = (Vector2)((Complex)(endOuter - start) * delta) + start;

            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
            {
                A = new Vector3(endInner, 0),
                B = new Vector3(endOuter, 0),
                C = new Vector3(nextInner, 0),
                Texture = renderer.WhiteTexture,
                Color = hovered ? hoverColor : defaultColor
            });

            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
            {
                A = new Vector3(endOuter, 0),
                B = new Vector3(nextOuter, 0),
                C = new Vector3(nextInner, 0),
                Texture = renderer.WhiteTexture,
                Color = hovered ? hoverColor : defaultColor
            });

            endInner = nextInner;
            endOuter = nextOuter;
        }
    }
}