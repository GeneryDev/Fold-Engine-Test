using FoldEngine.Resources;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Styles;

[Resource("button_style","styles/button")]
public class ButtonStyle : Resource
{
    public static readonly ButtonStyle Default = new ButtonStyle();
    
    public Color NormalColor = new Color(37, 37, 38);
    public Color RolloverColor = Color.CornflowerBlue;
    public Color PressedColor = new Color(63, 63, 70);
    
    public int FontSize = 14;
    public Color TextColor = Color.White;
    
    public float IconMaxWidth = 0;
    public float IconTextSeparation = 8;
    
    public float MarginTop = 0;
    public float MarginLeft = 0;
    public float MarginRight = 0;
    public float MarginBottom = 0;
}