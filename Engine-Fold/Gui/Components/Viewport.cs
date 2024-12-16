using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control.viewport")]
[ComponentInitializer(typeof(Viewport))]
public struct Viewport
{
    public string RenderGroupName;
    public string RenderLayerName;
    
    [DoNotSerialize] public Point MousePos;
    
    [DoNotSerialize] [EntityId] public long HoverTargetId = -1;

    [EntityId] public long FocusOwnerId = -1;
    public List<long> FocusableAncestorIds = new();

    public Viewport()
    {
        RenderGroupName = "main";
        RenderLayerName = "screen";
    }

    public IRenderingLayer GetLayer(IRenderingUnit renderer)
    {
        if (renderer.Groups.TryGetValue(RenderGroupName, out var group))
        {
            return group[RenderLayerName];
        }

        return null;
    }
}