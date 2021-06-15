using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Interfaces
{
    public interface IRenderingUnit {
        IGameCore Core { get; }
        
        TextureManager Textures { get; set; }
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
