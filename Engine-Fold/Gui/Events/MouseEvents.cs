using System;
using FoldEngine.Events;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Events;

[Event("fold:mouse_entered")]
public struct MouseEnteredEvent
{
    [EntityId] public long EntityId;
    public Point Position;

    public MouseEnteredEvent(long entityId, Point position)
    {
        EntityId = entityId;
        Position = position;
    }
}

[Event("fold:mouse_exited")]
public struct MouseExitedEvent
{
    [EntityId] public long EntityId;
    public Point Position;

    public MouseExitedEvent(long entityId, Point position)
    {
        EntityId = entityId;
        Position = position;
    }
}

[Event("fold:mouse_button")]
public struct MouseButtonEvent
{
    [EntityId] public long EntityId;
    public Point Position;
    public MouseEventType Type;
    public int Button;
    public long When;

    public bool Consumed;

    public const int LeftButton = 0;
    public const int MiddleButton = 1;
    public const int RightButton = 2;

    public const int MaxButtons = 3;

    public void Consume()
    {
        Consumed = true;
    }
}

public enum MouseEventType
{
    Pressed,
    Released
}
