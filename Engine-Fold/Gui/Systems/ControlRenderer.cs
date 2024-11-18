using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control_renderer", ProcessingCycles.Render, true)]
public class ControlRenderer : GameSystem
{
    private ComponentIterator<Viewport> _viewports;
    private ComponentIterator<Control> _controls;

    private List<RenderableKey> _entitiesToRender = new List<RenderableKey>();

    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        IRenderingLayer layer = null;

        _viewports.Reset();
        while (_viewports.Next())
        {
            ref var viewport = ref _viewports.GetComponent();
            layer = viewport.GetLayer(renderer);
            break;
        }

        if (layer == null) return;
        
        _entitiesToRender.Clear();
        
        _controls.Reset();
        while (_controls.Next())
        {
            ref var control = ref _controls.GetCoComponent<Control>();
            
            float sortKey = control.ZOrder;
            _entitiesToRender.Add(new RenderableKey()
            {
                EntityId = _controls.GetEntityId(),
                SortKey = sortKey
            });
        }
        
        _entitiesToRender.Sort((a, b) => Math.Sign(a.SortKey - b.SortKey));
        
        for (var i = 0; i < _entitiesToRender.Count; i++)
        {
            var entity = new Entity(Scene, _entitiesToRender[i].EntityId);
            ref var transform = ref entity.Transform;
            ref var control = ref entity.GetComponent<Control>();
            if (entity.HasComponent<BoxControl>())
            {
                RenderBox(renderer, layer, ref transform, ref control, ref entity.GetComponent<BoxControl>());
            }
            if (entity.HasComponent<LabelControl>())
            {
                RenderLabel(renderer, layer, ref transform, ref control, ref entity.GetComponent<LabelControl>());
            }
        }
    }

    private void RenderBox(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform, ref Control control, ref BoxControl box)
    {
        var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
        
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = box.Color,
            DestinationRectangle = bounds,
            Z = -control.ZOrder
        });
    }

    private void RenderLabel(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform, ref Control control, ref LabelControl label)
    {
        var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());

        if (label.UpdateRenderedText(renderer))
        {
            control.ComputedMinimumSize = new Vector2(label.RenderedText.Width, label.RenderedText.Height);
            control.RequestLayout = true;
        }
        ref RenderedText renderedText = ref label.RenderedText;
        if (!renderedText.HasValue) return;

        float textWidth = renderedText.Width;

        int totalWidth = (int)textWidth;

        int x;
        switch (label.Alignment)
        {
            case Alignment.Begin:
                x = bounds.X;
                break;
            case Alignment.Center:
                x = bounds.Center.X - totalWidth / 2;
                break;
            case Alignment.End:
                x = bounds.X + bounds.Width - totalWidth;
                break;
            default:
                x = bounds.X;
                break;
        }

        Point offset = Point.Zero;

        renderedText.DrawOnto(layer.Surface, new Point(x, bounds.Center.Y - renderedText.Height / 2 + label.FontSize) + offset,
            label.Color, z: -control.ZOrder);
        // layer.Surface.Draw(new DrawRectInstruction
        // {
        //     Texture = renderer.WhiteTexture,
        //     Color = Color.White,
        //     DestinationRectangle = new Rectangle(new Point(x - 2, bounds.Center.Y - renderedText.Height / 2 + label.FontSize), new Point(2, 2)),
        //     Z = control.ZOrder
        // });
    }

    private struct RenderableKey
    {
        public long EntityId;
        public float SortKey;
    }
}