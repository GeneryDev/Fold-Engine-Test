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
        TextureManager TextureManager { get; set; }

        Point ScreenSize { get; }
        Dictionary<string, IRenderingLayer> Layers { get; }

        void LoadContent();

        void Draw();
    }
}
