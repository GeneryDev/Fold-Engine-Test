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
public partial class ControlRenderer : GameSystem
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
            if (entity.HasComponent<ButtonControl>())
            {
                RenderButton(renderer, layer, ref transform, ref control, ref entity.GetComponent<ButtonControl>());
            }
        }
    }

    private struct RenderableKey
    {
        public long EntityId;
        public float SortKey;
    }
}