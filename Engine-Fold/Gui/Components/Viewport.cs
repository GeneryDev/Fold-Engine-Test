using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control.viewport")]
[ComponentInitializer(typeof(Viewport), nameof(InitializeComponent))]
public struct Viewport
{
    public string RenderGroupName;
    public string RenderLayerName;
    
    [DoNotSerialize] public Point MousePos;
    
    [DoNotSerialize] [EntityId] public long HoverTargetId;
    [DoNotSerialize] [EntityId] public long PrevHoverTargetId;

    public Viewport()
    {
        HoverTargetId = -1;
        PrevHoverTargetId = -1;
        RenderGroupName = "main";
        RenderLayerName = "screen";
    }

    public static Viewport InitializeComponent(Scene scene, long entityId)
    {
        return new Viewport();
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