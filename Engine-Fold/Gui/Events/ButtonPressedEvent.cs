using FoldEngine.Events;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Events;

[Event("fold:control.button_pressed")]
public struct ButtonPressedEvent
{
    [EntityId] public long EntityId;
    public Point Position;
}