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
        private readonly WooferGameController Controller;

        public TextureManager TextureManager { get; set; }
        public Point ScreenSize { get; private set; } = new Point(1280, 720);

        public Dictionary<string, IRenderingLayer> Layers { get; private set; } = new Dictionary<string, IRenderingLayer>();

        public WooferRenderingUnit(WooferGameController controller)
        {
            Controller = controller;
            Layers["level"] = new RenderingLayer() { Name = "level", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, ScreenSize) };
            Layers["hud"] = new RenderingLayer() { Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, ScreenSize) };
        }

        Texture2DWrapper testSprite;

        public void Draw()
        {
            Layers["level"].Surface.Draw(testSprite, new Rectangle((int)Time.TotalTime, 16, 16, 16), null, Color.White, (float)Math.PI / 3, Vector2.Zero);
            Layers["level"].Surface.Draw(testSprite, new Rectangle((int)Time.TotalTime, 0, 8, 8), Color.White);
            Layers["hud"].Surface.Draw(testSprite, new Rectangle((int)Time.TotalTime, 16, 16, 16), Color.White);
        }
        public void LoadContent()
        {
            testSprite = TextureManager.LoadSprite("Textures/test");
        }
    }
}
