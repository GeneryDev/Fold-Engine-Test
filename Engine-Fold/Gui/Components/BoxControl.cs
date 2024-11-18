using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control.box", traits: [typeof(Control), typeof(MousePickable)])]
public struct BoxControl
{
    public Color Color;
}