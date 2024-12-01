using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;

namespace FoldEngine.Gui.Components;

[Component("fold:control.tooltip_provider", traits: [typeof(TooltipProvider)])]
[ComponentTrait("#fold:tooltip_provider")]
public struct TooltipProvider
{
}


[Component("fold:control.simple_tooltip", traits: [typeof(TooltipProvider)])]
public struct SimpleTooltip
{
    public string Text;
}