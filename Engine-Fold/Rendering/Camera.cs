using FoldEngine.Components;

namespace FoldEngine.Rendering;

[Component("fold:camera")]
public struct Camera
{
    public string RenderToLayer;
    public float SnapPosition;
}