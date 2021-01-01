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
    public interface IRenderingUnit
    {
        TextureManager Textures { get; set; }

        Point ScreenSize { get; }
        Dictionary<string, IRenderingLayer> Layers { get; }
        
        ITexture WhiteTexture { get; }

        void LoadContent();

        void Render();
        
        IRenderingLayer GizmoLayer { get; }
    }
}
