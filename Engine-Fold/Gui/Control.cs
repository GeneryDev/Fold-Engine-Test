using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui;

[Component("fold:control")]
public struct Control
{
    public Vector2 Size;

    public float ZOrder;

    public bool RequestLayout;
}