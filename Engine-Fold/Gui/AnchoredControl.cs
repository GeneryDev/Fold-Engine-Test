using FoldEngine.Components;

namespace FoldEngine.Gui;

[Component("fold:control.anchored")]
public struct AnchoredControl
{
    public float AnchorLeft;
    public float AnchorTop;
    public float AnchorRight;
    public float AnchorBottom;

    public float OffsetLeft;
    public float OffsetTop;
    public float OffsetRight;
    public float OffsetBottom;
    
    public GrowDirection GrowHorizontal;
    public GrowDirection GrowVertical;

    public enum GrowDirection
    {
        Begin,
        End,
        Both
    }
}