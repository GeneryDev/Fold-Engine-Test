using FoldEngine.Components;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Components;

[Component("fold:control.anchored", traits: [typeof(Control)])]
[ComponentInitializer(typeof(AnchoredControl), nameof(InitializeComponent))]
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

    public AnchoredControl()
    {
        GrowHorizontal = GrowVertical = GrowDirection.Both;
    }
    
    /// <summary>
    ///     Returns an initialized anchored component with all its correct default values.
    /// </summary>
    public static AnchoredControl InitializeComponent(Scene scene, long entityId)
    {
        return new AnchoredControl();
    }
}