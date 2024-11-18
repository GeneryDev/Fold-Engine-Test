using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;

namespace FoldEngine.Gui.Components.Containers;

[Component("fold:control.flow_container", traits: [typeof(Control), typeof(Container)])]
public struct FlowContainer
{
    public bool Vertical;
    public Alignment Alignment;

    public float HSeparation;
    public float VSeparation;
}