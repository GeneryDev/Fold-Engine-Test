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
    public class WooferRenderingUnit : IRenderingUnit {
        private readonly WooferGameCore _core;

        public TextureManager Textures { get; set; }
        public FontManager Fonts { get; set; }
        public Point WindowSize { get; set; } = new Point(1280, 720);

        public Dictionary<string, IRenderingLayer> Layers { get; private set; } =
            new Dictionary<string, IRenderingLayer>();

        public ITexture WhiteTexture { get; set; }

        public IRenderingLayer WorldLayer => Layers["world"];
        public IRenderingLayer WindowLayer => Layers["screen"];
        public IRenderingLayer GizmoLayer => Layers["gizmos"];

        public WooferRenderingUnit(WooferGameCore core) {
            _core = core;
            Layers["world"] = new RenderingLayer(this) {
                Name = "world", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, WindowSize),
                LogicalSize = WindowSize.ToVector2()
            };
            Layers["gizmos"] = new RenderingLayer(this) {
                Name = "gizmos", LayerSize = WindowSize, Destination = new Rectangle(Point.Zero, WindowSize),
                LogicalSize = WindowSize.ToVector2()
            };
            Layers["screen"] = new RenderingLayer(this) {
                Name = "screen", LayerSize = WindowSize, Destination = new Rectangle(Point.Zero, WindowSize),
                LogicalSize = WindowSize.ToVector2()
            };
            Layers["hud"] = new RenderingLayer(this) {
                Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, WindowSize),
                LogicalSize = WindowSize.ToVector2()
            };
        }

        public void LoadContent() {
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




            TextureAtlas editorAtlas = Textures.CreateAtlas("editor");
            editorAtlas.AddTexture("cog", "editor/cog");
            editorAtlas.AddTexture("blank", "editor/blank");
            editorAtlas.AddTexture("triangle.right", "editor/triangle.right");
            editorAtlas.AddTexture("triangle.down", "editor/triangle.down");
            editorAtlas.Pack();




            Textures.LoadTexture("fonts/default/ascii");
            Fonts.LoadFont("default");
        }
    }
}
