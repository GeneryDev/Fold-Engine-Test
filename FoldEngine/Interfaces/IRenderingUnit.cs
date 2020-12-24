using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Graphics;

namespace FoldEngine.Interfaces
{
    public interface IRenderingUnit
    {
        TextureManager Textures { get; set; }

        Point ScreenSize { get; }
        Dictionary<string, IRenderingLayer> Layers { get; }

        void LoadContent();

        void Render();
    }
}
