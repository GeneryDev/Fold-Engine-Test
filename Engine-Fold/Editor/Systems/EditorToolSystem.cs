using System.Collections.Generic;
using FoldEngine.Editor.Tools;
using FoldEngine.Systems;

namespace FoldEngine.Editor.Systems;

[GameSystem("fold:editor.tools", ProcessingCycles.Input | ProcessingCycles.Render)]
public class EditorToolSystem : GameSystem
{
    public readonly List<EditorTool> Tools = new List<EditorTool>();
    public EditorTool ForcedTool;
    public EditorTool SelectedTool;

    private void SetupTools()
    {
        // Tools.Add(new HandTool(this));
        // Tools.Add(new MoveTool(this));
        // Tools.Add(new ScaleTool(this));
        // Tools.Add(new RotateTool(this));
        // Tools.Add(SelectedTool = new SelectTool(this));
    }
}