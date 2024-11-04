using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorTestView : EditorView
{
    public EditorTestView()
    {
        Icon = new ResourceIdentifier("editor/pause");
    }

    public override string Name => "Test";

    public override bool UseMargin => false;
    public override Color? BackgroundColor => Color.Black;

    public override void Render(IRenderingUnit renderer)
    {
        DrawQuadInstruction instructionA = new DrawRectInstruction(renderer.WhiteTexture,
            new Rectangle(new Point(50, 50) + ContentPanel.Bounds.Location, new Point(100, 100)))
        {
            Color = new Color(255, 0, 0)
        };
        DrawQuadInstruction instructionB = new DrawRectInstruction(renderer.WhiteTexture,
            new Rectangle(new Point(100, 100) + ContentPanel.Bounds.Location, new Point(100, 100)))
        {
            Color = new Color(0, 255, 0)
        };

        float zA = 25f;
        float zB = 26f;

        instructionA.A.Z = instructionA.B.Z = instructionA.C.Z = instructionA.D.Z = zA;
        instructionB.A.Z = instructionB.B.Z = instructionB.C.Z = instructionB.D.Z = zB;

        instructionA.A.Z = 28f;

        ContentPanel.Environment.BaseLayer.Surface.Draw(instructionA);
        ContentPanel.Environment.BaseLayer.Surface.Draw(instructionB);
    }
}