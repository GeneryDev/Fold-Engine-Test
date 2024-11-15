using FoldEngine.Components;

namespace FoldEngine.Gui;

[Component("fold:control.flow_container")]
public struct FlowContainer
{
    public bool Vertical;

    public float HSeparation;
    public float VSeparation;
}