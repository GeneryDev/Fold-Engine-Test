using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public abstract class EditorView
{
    public GuiPanel ContentPanel;

    public ResourceIdentifier Icon;
    public Scene Scene;
    public IGameCore Core => Scene.Core;
    public ResourceCollections EditorResources => Scene.Resources;
    public Scene EditingScene => Scene;
    public abstract string Name { get; }

    public virtual bool UseMargin => true;
    public virtual Color? BackgroundColor => null;

    public virtual void Initialize()
    {
    }

    public abstract void Render(IRenderingUnit renderer);

    public virtual void EnsurePanelExists(GuiEnvironment environment)
    {
        if (ContentPanel == null) ContentPanel = new GuiPanel(environment);
    }
}