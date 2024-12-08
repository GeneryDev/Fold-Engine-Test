using System;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Systems;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools;

public abstract class EditorTool
{
    protected readonly EditorToolSystem System;
    public ResourceIdentifier Icon;


    public EditorTool(EditorToolSystem system)
    {
        System = system;
    }

    protected Scene Scene => System.Scene;
    public IGameCore Core => Scene.Core;
    public Vector2 MousePos => System.MousePos;

    public abstract void OnInput();
    public abstract void OnMousePressed(ref MouseEvent e);
    public abstract void OnMouseReleased(ref MouseEvent e);

    public virtual void Render(IRenderingUnit renderer)
    {
    }
}