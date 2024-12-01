using FoldEngine.Events;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Events;

[Event("fold:control.tooltip_requested", EventFlushMode.Immediate)]
public struct TooltipRequestedEvent
{
    [EntityId] public long EntityId = -1;
    public Point Position;
    public Point GlobalPosition;
    
    [EventOutput] public string TooltipText;

    public TooltipRequestedEvent()
    {
    }
}

[Event("fold:control.tooltip_build_requested", EventFlushMode.Immediate)]
public struct TooltipBuildRequestedEvent
{
    [EntityId] public long SourceEntityId = -1;
    [EntityId] public long TooltipEntityId = -1;
    public Point Position;
    public Point GlobalPosition;
    
    public string TooltipText;
    
    [EventOutput] public Point Offset;

    public TooltipBuildRequestedEvent()
    {
    }
}