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
        public IGameCore Core { get; private set; }
        private Point _windowSize = new Point(1280, 720);

        public TextureManager Textures { get; set; }
        public FontManager Fonts { get; set; }

        public Point WindowSize {
            get => _windowSize;
            set {
                Point oldSize = _windowSize;
                _windowSize = value;
                if(Core.FoldGame != null) {
                    (Core.FoldGame.Graphics.PreferredBackBufferWidth, Core.FoldGame.Graphics.PreferredBackBufferHeight) =
                        value;
                    if(value != oldSize) {
                        foreach(RenderGroup group in Groups.Values) {
                            group.WindowSizeChanged(oldSize, value);
                        }
                    }
                    Core.FoldGame.Graphics.ApplyChanges();
                }
            }
        }

        public ITexture WhiteTexture { get; private set; }
        
        public Dictionary<string, RenderGroup> Groups { get; private set; } = new Dictionary<string, RenderGroup>();

        public RenderGroup RootGroup { get; set; }
        public RenderGroup MainGroup { get; set; }

        public IRenderingLayer WindowLayer => MainGroup["screen"];
        public IRenderingLayer WorldLayer => MainGroup["world"];
        public IRenderingLayer GizmoLayer => MainGroup["gizmos"];

        public WooferRenderingUnit(WooferGameCore core) {
            Core = core;
        }

        public void Initialize() {
            var mainSize = new Point(1280, 720); 
            
            Groups["main"] = RootGroup = MainGroup = new RenderGroup(this) {
                Size = mainSize,
                ["world"] = new RenderingLayer(this) {
                    Name = "world", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, mainSize),
                    LogicalSize = mainSize.ToVector2(),
                    Color = new Color(56, 56, 56)
                },
                ["gizmos"] = new RenderingLayer(this) {
                    Name = "gizmos", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    LogicalSize = mainSize.ToVector2()
                },
                ["screen"] = new RenderingLayer(this) {
                    Name = "screen", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    LogicalSize = mainSize.ToVector2()
                },
                ["hud"] = new RenderingLayer(this) {
                    Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, mainSize),
                    LogicalSize = mainSize.ToVector2()
                }
            };

            var fullSize = new Point(1920, 1040);

            // Groups["editor"] = RootGroup = new RenderGroup(this) {
            //     Size = fullSize
            // };
            // RootGroup.AddDependency(new RenderGroup.Dependency() {
            //     Group = MainGroup,
            //     Destination = new Rectangle(new Point(50, 50), mainSize)
            // });

            Groups["root"] = RootGroup = new ResizableRenderGroup(this) {
                Size = mainSize
            };
            RootGroup.AddDependency(new RenderGroup.Dependency() {
                Group = MainGroup
            });

            WindowSize = RootGroup.Size;
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

        public Rectangle GetGroupBounds(RenderGroup renderGroup) {
            Rectangle? bounds = RootGroup.GetBounds(renderGroup);
            if(bounds.HasValue) return bounds.Value;
            throw new ArgumentException($"RenderGroup {renderGroup} is not present in the current RenderGroup hierarchy.");
        }
    }
}
