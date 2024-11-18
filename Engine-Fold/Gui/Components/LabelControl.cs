using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control.label", traits: [typeof(Control), typeof(MousePickable)])]
[ComponentInitializer(typeof(LabelControl), nameof(InitializeComponent))]
public struct LabelControl
{
    public string Text;
    public int FontSize;
    public Color Color;
    public Alignment Alignment;

    [DoNotSerialize] [HideInInspector] public RenderedText RenderedText;
    
    public LabelControl()
    {
        Text = "";
        FontSize = 14;
        Color = Color.White;
        Alignment = Alignment.Begin;
    }
    
    /// <summary>
    ///     Returns an initialized label component with all its correct default values.
    /// </summary>
    public static LabelControl InitializeComponent(Scene scene, long entityId)
    {
        return new LabelControl();
    }

    public bool UpdateRenderedText(IRenderingUnit renderer)
    {
        if (RenderedText.HasValue && RenderedText.Text == Text && RenderedText.Size == FontSize)
        {
            // already up to date
            return false;
        }
        renderer.Fonts["default"].RenderString(Text, out RenderedText, FontSize);
        return true;
    }
}