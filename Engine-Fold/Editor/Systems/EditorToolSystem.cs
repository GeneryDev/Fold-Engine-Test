using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.Tools;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Systems;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems;

[GameSystem("fold:editor.tools", ProcessingCycles.Input | ProcessingCycles.Render)]
public class EditorToolSystem : GameSystem
{
    public readonly List<EditorTool> Tools = new List<EditorTool>();
    public EditorTool ForcedTool;
    public EditorTool SelectedTool;

    public EditorTool ActiveTool => ForcedTool ?? SelectedTool;

    public Vector2 MousePos;

    private ComponentIterator<Viewport> _viewports;

    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        
        SetupTools();
    }

    private void SetupTools()
    {
        Tools.Add(new HandTool(this));
        Tools.Add(new MoveTool(this));
        Tools.Add(new ScaleTool(this));
        Tools.Add(new RotateTool(this));
        Tools.Add(SelectedTool = new SelectTool(this));
    }

    public override void OnInput()
    {
        _viewports.Reset();
        while (_viewports.Next())
        {
            MousePos = _viewports.GetComponent().MousePos.ToVector2();
        }

        // These are currently called by EditorSceneView
        // ActiveTool?.OnInput();
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        // These are currently called by EditorSceneView
        // ActiveTool?.Render(renderer);
    }
}