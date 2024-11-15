using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui;

[Component("fold:control")]
public struct Control
{
    public Vector2 Size;

    public float ZOrder;

    public bool RequestLayout;

    public bool UseAnchors;
    
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float AnchorLeft;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float AnchorTop;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float AnchorRight;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float AnchorBottom;

    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float OffsetLeft;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float OffsetTop;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float OffsetRight;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public float OffsetBottom;
    
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public GrowDirection GrowHorizontal;
    [ShowOnlyIf(nameof(UseAnchors), true)]
    public GrowDirection GrowVertical;

    public enum GrowDirection
    {
        Begin,
        End,
        Both
    }
}