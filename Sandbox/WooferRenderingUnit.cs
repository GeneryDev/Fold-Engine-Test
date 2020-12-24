using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Util;

using Microsoft.Xna.Framework;

namespace Woofer
{
    public class WooferRenderingUnit : IRenderingUnit
    {
        private readonly WooferGameController _controller;

        public TextureManager Textures { get; set; }
        public Point ScreenSize { get; private set; } = new Point(1280, 720);

        public Dictionary<string, IRenderingLayer> Layers { get; private set; } = new Dictionary<string, IRenderingLayer>();

        public WooferRenderingUnit(WooferGameController controller)
        {
            _controller = controller;
            Layers["level"] = new RenderingLayer() { Name = "level", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, ScreenSize) };
            Layers["hud"] = new RenderingLayer() { Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, ScreenSize) };
        }


        public void Render()
        {
            _controller.ActiveScene?.Render(this);
        }
        public void LoadContent()
        {
            Textures.LoadSprite("test");
        }
    }
}
