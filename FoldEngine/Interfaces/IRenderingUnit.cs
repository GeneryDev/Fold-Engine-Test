using Microsoft.Xna.Framework;

using System.Collections.Generic;

using FoldEngine.Graphics;

namespace FoldEngine.Interfaces
{
    public interface IRenderingUnit {
        IGameCore Core { get; }
        
        EffectManager Effects { get; set; }
        FontManager Fonts { get; set; }

        Point WindowSize { get; set; }
        RenderGroup RootGroup { get; set; }
        RenderGroup MainGroup { get; set; }
        Dictionary<string, RenderGroup> Groups { get; }
        
        ITexture WhiteTexture { get; }

        void LoadContent();
        
        IRenderingLayer WorldLayer { get; }
        IRenderingLayer WindowLayer { get; }
        IRenderingLayer GizmoLayer { get; }

        Rectangle GetGroupBounds(RenderGroup renderGroup);
        void Initialize();
    }
}
