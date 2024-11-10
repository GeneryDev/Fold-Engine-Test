using FoldEngine.Editor.ImmediateGui;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Tools;

public abstract class EditorTool
{
    protected readonly EditorEnvironment Environment;
    public ResourceIdentifier Icon;


    public EditorTool(EditorEnvironment environment)
    {
        Environment = environment;
    }

    protected Scene Scene => Environment.Scene;
    public IGameCore Core => Scene.Core;

    public abstract void OnInput(ControlScheme controls);
    public abstract void OnMousePressed(ref MouseEvent e);
    public abstract void OnMouseReleased(ref MouseEvent e);

    public virtual void Render(IRenderingUnit renderer)
    {
    }
}