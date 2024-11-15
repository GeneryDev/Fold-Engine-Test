using System;
using System.Collections.Generic;
using System.Net;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Rendering;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui;

[GameSystem("fold:control_renderer", ProcessingCycles.Render, true)]
public class ControlRenderer : GameSystem
{
    private ComponentIterator<BoxControl> _boxes;

    private List<RenderableKey> _entitiesToRender = new List<RenderableKey>();

    public override void Initialize()
    {
        _boxes = CreateComponentIterator<BoxControl>(IterationFlags.Ordered);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        var layer = renderer.WorldLayer;
        
        _boxes.Reset();
        _entitiesToRender.Clear();
        
        while (_boxes.Next())
        {
            if(!_boxes.HasCoComponent<Control>()) continue;
            ref var control = ref _boxes.GetCoComponent<Control>();
            ref var box = ref _boxes.GetComponent();
            
            float sortKey = control.ZOrder;
            _entitiesToRender.Add(new RenderableKey()
            {
                EntityId = _boxes.GetEntityId(),
                SortKey = sortKey
            });
        }
        
        _entitiesToRender.Sort((a, b) => Math.Sign(a.SortKey - b.SortKey));
        
        
        for (var i = 0; i < _entitiesToRender.Count; i++)
        {
            var entity = new Entity(Scene, _entitiesToRender[i].EntityId);
            ref Transform transform = ref entity.Transform;
            ref Control control = ref entity.GetComponent<Control>();
            ref BoxControl box = ref entity.GetComponent<BoxControl>();

            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = box.Color,
                DestinationRectangle = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint())
            });
        }
    }

    private struct RenderableKey
    {
        public long EntityId;
        public float SortKey;
    }
}