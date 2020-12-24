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
            Textures.LoadSprite("ancient_debris_side");
            Textures.LoadSprite("armor_stand");
            Textures.LoadSprite("bamboo_stem");
            Textures.LoadSprite("barrel_top");
            Textures.LoadSprite("beacon");
            Textures.LoadSprite("bed_feet_end");
            Textures.LoadSprite("end_portal_colors");
            Textures.LoadSprite("guardian");
            Textures.LoadSprite("pillager");
            Textures.LoadSprite("witch");
            Textures.LoadSprite("clouds");
            Textures.LoadSprite("campfire_smoke");
            Textures.LoadSprite("soul");
            Textures.LoadSprite("particles");
            
            TextureAtlas atlas = Textures.CreateAtlas("main");
            atlas.AddTexture("test", Textures["test"]);
            atlas.AddTexture("ancient_debris_side", Textures["ancient_debris_side"]);
            atlas.AddTexture("armor_stand", Textures["armor_stand"]);
            atlas.AddTexture("bamboo_stem", Textures["bamboo_stem"]);
            atlas.AddTexture("barrel_top", Textures["barrel_top"]);
            atlas.AddTexture("beacon", Textures["beacon"]);
            atlas.AddTexture("bed_feet_end", Textures["bed_feet_end"]);
            atlas.AddTexture("end_portal_colors", Textures["end_portal_colors"]);
            atlas.AddTexture("guardian", Textures["guardian"]);
            atlas.AddTexture("pillager", Textures["pillager"]);
            atlas.AddTexture("witch", Textures["witch"]);
            atlas.AddTexture("clouds", Textures["clouds"]);
            atlas.AddTexture("campfire_smoke", Textures["campfire_smoke"]);
            atlas.AddTexture("soul", Textures["soul"]);
            atlas.AddTexture("particles", Textures["particles"]);
            
            atlas.Pack();
        }
    }
}
