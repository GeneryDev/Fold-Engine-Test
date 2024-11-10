using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools;

public class MoveTool : SelectTool
{
    private bool _dragging;
    private Transform _movePivot;
    private readonly List<Vector2> _pressEntityPivotPosition = new List<Vector2>();
    private Vector2 _pressMousePivotPosition;

    private Vector2 _pressPivotPosition;

    private Vector2 _selectedGizmo;
    private readonly List<SetEntityTransformTransaction> _transactions = new List<SetEntityTransformTransaction>();

    public MoveTool(EditorEnvironment environment) : base(environment)
    {
        Icon = EditorIcons.Move;
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        if (_selectedGizmo != default)
        {
            var editorBase = Scene.Systems.Get<EditorBase>();
            var editingTab = editorBase.CurrentTab;
            if (editingTab.Scene == null) return;
            
            Vector2 mouseWorldPos =
                editingTab.Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                    Environment.Renderer.GizmoLayer.WindowToLayer(e.Position.ToVector2())));
            _pressPivotPosition = _movePivot.Position;
            _pressMousePivotPosition = _movePivot.Relativize(mouseWorldPos);

            _transactions.Clear();
            _pressEntityPivotPosition.Clear();
            foreach (long entityId in editingTab.EditingEntity)
            {
                if (entityId == -1) continue;

                var entity = new Entity(editingTab.Scene, entityId);

                Vector2 relativeEntityPos = _movePivot.Relativize(entity.Transform.Position);

                _pressEntityPivotPosition.Add(relativeEntityPos);

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
        if (!_movePivot.IsNotNull) _movePivot = Transform.InitializeComponent(Scene, 0);
    }

    public override void Render(IRenderingUnit renderer)
    {
        EnsurePivotExists();
        if (!_dragging) _selectedGizmo = default;

        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentTab;
        if (editingTab.Scene == null) return;

        bool any = false;
        
        if (_dragging)
        {
            Vector2 mouseWorldPos =
                editingTab.Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                    Environment.Renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2())));

            _movePivot.Position = mouseWorldPos;
            _movePivot.Position = _movePivot.Apply(-_pressMousePivotPosition);

            Vector2 pivotDelta = _movePivot.Position - _pressPivotPosition;

            Vector2 filteredDelta =
                ((Complex)pivotDelta / _movePivot.RotationComplex).ScaleAxes(_selectedGizmo.X,
                    _selectedGizmo.Y)
                * _movePivot.RotationComplex;

            _movePivot.Position = _pressPivotPosition + filteredDelta;

            int i = 0;
            foreach (long entityId in editingTab.EditingEntity)
            {
                if (entityId == -1) continue;
                any = true;

                var entity = new Entity(editingTab.Scene, entityId);

                entity.Transform.Position = _movePivot.Position;
                entity.Transform.Position = _movePivot.Apply(_pressEntityPivotPosition[i]);

                _transactions[i].UpdateAfter(entity.Transform.CreateSnapshot());

                i++;
            }
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
                _movePivot.Rotation = entity.Transform.Rotation;
            }

            if (any) _movePivot.LocalPosition /= editingTab.EditingEntity.Count;
        }

        if (any)
        {
            Vector2 origin = _movePivot.LocalPosition;
            Complex rotation = (_movePivot.Apply(Vector2.UnitX) - origin).Normalized();

            RenderArrow(renderer,
                origin,
                origin + (Vector2)((Complex)Vector2.UnitX * rotation),
                Color.Red,
                new Color(255,
                    200,
                    200),
                out bool hoveredX,
                _dragging ? _selectedGizmo.X > 0 : (bool?)null,
                editingTab.Scene.MainCameraTransform,
                100);
            RenderArrow(renderer,
                origin,
                origin + (Vector2)((Complex)Vector2.UnitY * rotation),
                Color.Lime,
                new Color(200,
                    255,
                    200),
                out bool hoveredY,
                _dragging ? _selectedGizmo.Y > 0 : (bool?)null,
                editingTab.Scene.MainCameraTransform,
                100);

            if (hoveredX) _selectedGizmo.X = 1;
            if (hoveredY) _selectedGizmo.Y = 1;
        }
    }

    private void RenderArrow(
        IRenderingUnit renderer,
        Vector2 start,
        Vector2 end,
        Color defaultColor,
        Color hoverColor,
        out bool hovered,
        bool? forceHoverState,
        Transform cameraTransform,
        float fixedLength = 0)
    {
        start = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(start));
        end = renderer.GizmoLayer.CameraToLayer(cameraTransform.Relativize(end));

        if (fixedLength > 0) end = (end - start).Normalized() * fixedLength + start;

        Vector2 dir = (end - start).Normalized();
        Complex dirComplex = dir;

        float thickness = 2;
        float headLength = 20;
        float headWidth = 16;
        float hoverDistance = 16;

        //Check hover
        if (!forceHoverState.HasValue)
        {
            var line = new Line(start, end);
            Vector2 mousePosLayerSpace = renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2());
            hovered = line.DistanceFromPoint(mousePosLayerSpace, true) <= hoverDistance;
        }
        else
        {
            hovered = forceHoverState.Value;
        }

        //Render line

        renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
        {
            A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            Texture = renderer.WhiteTexture,
            Color = hovered ? hoverColor : defaultColor
        });
        renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
        {
            A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            Texture = renderer.WhiteTexture,
            Color = hovered ? hoverColor : defaultColor
        });

        // Render head
        renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction
        {
            A = new Vector3(end, 0),
            B = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
            C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
            Texture = renderer.WhiteTexture,
            Color = hovered ? hoverColor : defaultColor
        });
    }
}