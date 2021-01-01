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
using Microsoft.Xna.Framework.Graphics;

namespace Woofer
{
    public class WooferRenderingUnit : IRenderingUnit
    {
        private readonly WooferGameController _controller;

        public TextureManager Textures { get; set; }
        public Point ScreenSize { get; private set; } = new Point(1280, 720);

        public Dictionary<string, IRenderingLayer> Layers { get; private set; } = new Dictionary<string, IRenderingLayer>();

        public ITexture WhiteTexture { get; set; }

        public IRenderingLayer GizmoLayer => Layers["screen"];

        public WooferRenderingUnit(WooferGameController controller)
        {
            _controller = controller;
            Layers["level"] = new RenderingLayer() { Name = "level", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, ScreenSize) };
            Layers["hud"] = new RenderingLayer() { Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, ScreenSize) };
            Layers["screen"] = new RenderingLayer() { Name = "screen", LayerSize = ScreenSize, Destination = new Rectangle(Point.Zero, ScreenSize) };
        }


        public void Render()
        {
            _controller.ActiveScene?.Render(this);
        }
        
        public void LoadContent()
        {
            Textures.LoadTexture("test");
            Textures.LoadTexture("ancient_debris_side");
            Textures.LoadTexture("armor_stand");
            Textures.LoadTexture("bamboo_stem");
            Textures.LoadTexture("barrel_top");
            Textures.LoadTexture("beacon");
            Textures.LoadTexture("bed_feet_end");
            Textures.LoadTexture("end_portal_colors");
            Textures.LoadTexture("guardian");
            Textures.LoadTexture("pillager");
            Textures.LoadTexture("witch");
            Textures.LoadTexture("clouds");
            Textures.LoadTexture("campfire_smoke");
            Textures.LoadTexture("soul");
            Textures.LoadTexture("particles");
            Textures.LoadTexture("four");
            
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
            atlas.AddTexture("pixel", Textures["four"]);
            atlas.Pack();
            
            Textures.CreateSubTexture("main:soul", "start", new Rectangle(0, 0, 16, 16));
            Textures.CreateSubTexture("main:pixel", "black_transparent", new Rectangle(0, 0, 1, 1));
            Textures.CreateSubTexture("main:pixel", "white_transparent", new Rectangle(1, 0, 1, 1));
            Textures.CreateSubTexture("main:pixel", "black", new Rectangle(0, 1, 1, 1));
            WhiteTexture = Textures.CreateSubTexture("main:pixel", "white", new Rectangle(1, 1, 1, 1));
        }
    }
}
