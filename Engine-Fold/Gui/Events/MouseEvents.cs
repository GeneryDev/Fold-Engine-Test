using System;
using FoldEngine.Events;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Events;

[Event("fold:mouse_entered")]
public struct MouseEnteredEvent
{
    [EntityId] public long EntityId = -1;
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
    [EntityId] public long EntityId = -1;
    public Point Position;

    public MouseExitedEvent(long entityId, Point position)
    {
        EntityId = entityId;
        Position = position;
    }
}

[Event("fold:mouse_moved")]
public struct MouseMovedEvent
{
    [EntityId] public long EntityId = -1;
    public Point Position;
    public Point Delta;
    
    public bool Consumed;

    public MouseMovedEvent()
    {
    }

    public void Consume()
    {
        Consumed = true;
    }
}

[Event("fold:mouse_dragged")]
public struct MouseDraggedEvent
{
    [EntityId] public long EntityId = -1;
    public Point Position;
    public Point Delta;
    public int Button;
    
    public bool Consumed;

    public MouseDraggedEvent()
    {
    }

    public void Consume()
    {
        Consumed = true;
    }
}

[Event("fold:mouse_button")]
public struct MouseButtonEvent
{
    public const int LeftButton = 0;
    public const int MiddleButton = 1;
    public const int RightButton = 2;

    public const int MaxButtons = 3;
    
    [EntityId] public long EntityId = -1;
    public Point Position;
    public MouseButtonEventType Type;
    public int Button;

    public bool Consumed;

    public MouseButtonEvent()
    {
    }

    public void Consume()
    {
        Consumed = true;
    }
}

public enum MouseButtonEventType
{
    Pressed,
    Released
}
