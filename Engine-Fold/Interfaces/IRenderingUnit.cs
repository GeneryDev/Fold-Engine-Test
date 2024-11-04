using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Interfaces;

public interface IRenderingUnit
{
    IGameCore Core { get; }
    FontManager Fonts { get; set; }

    Point WindowSize { get; set; }
    RenderGroup RootGroup { get; set; }
    RenderGroup MainGroup { get; set; }
    Dictionary<string, RenderGroup> Groups { get; }

    ITexture WhiteTexture { get; }

    IRenderingLayer WorldLayer { get; }
    IRenderingLayer WindowLayer { get; }
    IRenderingLayer GizmoLayer { get; }

    void LoadContent();

    Rectangle GetGroupBounds(RenderGroup renderGroup);
    void Initialize();
}