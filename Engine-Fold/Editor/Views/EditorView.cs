using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public abstract class EditorView
{
    public GuiPanel ContentPanel;

    // Already accessible in environment?
    public Scene Scene;
    public IGameCore Core => Scene.Core;
    public ResourceCollections EditorResources => Scene.Resources;
    
    // For tab appearance
    public ResourceIdentifier Icon;
    public abstract string Name { get; }

    // For content appearance
    public virtual bool UseMargin => true;
    public virtual Color? BackgroundColor => null;

    
    public abstract void Render(IRenderingUnit renderer);

    public virtual void EnsurePanelExists(GuiEnvironment environment)
    {
        ContentPanel ??= new GuiPanel(environment);
    }
}