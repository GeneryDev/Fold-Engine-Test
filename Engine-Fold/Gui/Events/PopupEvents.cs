using FoldEngine.Events;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Events;

[Event("fold:control.tooltip_build_requested", EventFlushMode.Immediate)]
public struct PopupBuildRequestedEvent
{
    [EntityId] public long SourceEntityId = -1;
    [EntityId] public long TooltipEntityId = -1;
    public Point Position;
    public Point GlobalPosition;
    
    [EventOutput] public Point Gap;

    public PopupBuildRequestedEvent()
    {
    }
}