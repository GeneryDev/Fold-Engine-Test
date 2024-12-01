using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;

namespace FoldEngine.Gui.Components;

[Component("fold:control.simple_tooltip", traits: [typeof(TooltipProvider)])]
public struct SimpleTooltip
{
    public string Text;
}