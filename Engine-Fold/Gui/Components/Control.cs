using System;
using FoldEngine.Components;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control")]
[ComponentTrait("#fold:control")]
public struct Control
{
    public Vector2 Size;
    public Vector2 MinimumSize;
    public Vector2 ComputedMinimumSize;

    public float ZOrder;

    public bool RequestLayout;
    public MouseFilterMode MouseFilter;

    public Vector2 EffectiveMinimumSize => new Vector2(Math.Max(MinimumSize.X, ComputedMinimumSize.X),
        Math.Max(MinimumSize.Y, ComputedMinimumSize.Y));

    public enum MouseFilterMode
    {
        Auto,
        Ignore,
        Stop
    }
}