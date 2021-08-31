using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine;
using FoldEngine.Events;
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
        public EffectManager Effects { get; set; }
        public FontManager Fonts { get; set; }

        public Point WindowSize {
            get => _windowSize;
            set {
                Point oldSize = _windowSize;
                _windowSize = value;
                if(Core.FoldGame != null) {
                    if(value != oldSize) {
                        foreach(RenderGroup group in Groups.Values) {
                            group.WindowSizeChanged(oldSize, value);
                        }
                        Core.ActiveScene?.Events.Invoke(new WindowSizeChangedEvent(oldSize, value));
                    }
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
            Console.WriteLine("Initializing Rendering Unit");
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

            
            Groups["editor"] = RootGroup = new RenderGroup(this) {
                Size = mainSize,
                ["editor_gui"] = new RenderingLayer(this) {
                    Name = "editor_gui", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                },
                ["editor_gui_game"] = new DependencyRenderingLayer(0),
                ["editor_gui_overlay"] = new RenderingLayer(this) {
                    Name = "editor_gui_overlay", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                    
                }
            };
            Groups["editor"].AddDependency(new RenderGroup.Dependency() {
                Group = new ResizableRenderGroup(MainGroup) {
                    Size = mainSize
                },
                Destination = new Rectangle(Point.Zero, mainSize)
            });
            

            WindowSize = RootGroup.Size;
            UpdateWindowSize();
        }

        public void UpdateWindowSize() {
            (Core.FoldGame.Graphics.PreferredBackBufferWidth, Core.FoldGame.Graphics.PreferredBackBufferHeight) =
                WindowSize;
            Core.FoldGame.Graphics.ApplyChanges();
        }

        public void LoadContent() {
            Console.WriteLine("Loading Rendering Unit Content");
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
            editorAtlas.AddTexture("cube", "editor/cube");
            editorAtlas.AddTexture("hierarchy", "editor/hierarchy");
            editorAtlas.AddTexture("info", "editor/info");
            editorAtlas.AddTexture("checkmark", "editor/checkmark");
            editorAtlas.AddTexture("play", "editor/play");
            editorAtlas.AddTexture("pause", "editor/pause");
            editorAtlas.Pack();


            Console.WriteLine("Textures and atlases loaded");


            Console.WriteLine("Loading Fonts");
            
            Textures.LoadTexture("fonts/default.7/ascii");
            Textures.LoadTexture("fonts/default.9/ascii");
            Fonts.LoadFont("default.7");
            Fonts.LoadFont("default.9");
            
            Console.WriteLine("Fonts loaded");
        }

        public Rectangle GetGroupBounds(RenderGroup renderGroup) {
            Rectangle? bounds = RootGroup.GetBounds(renderGroup);
            if(bounds.HasValue) return bounds.Value;
            throw new ArgumentException($"RenderGroup {renderGroup} is not present in the current RenderGroup hierarchy.");
        }
    }

    public class DependencyRenderingLayer : IRenderingLayer {
        public int DependencyIndex = 0;
        public DependencyRenderingLayer(int index) {
            DependencyIndex = index;
        }

        public IRenderingUnit RenderingUnit { get; }
        public RenderGroup Group { get; set; }
        public string Name { get; }
        public Point LayerSize { get; }
        public Vector2 LogicalSize { get; }
        public Rectangle Destination { get; set; }
        public SamplerState Sampling { get; }
        public RenderSurface Surface { get; set; }
        public Color? Color { get; set; }
        public Vector2 CameraToLayer(Vector2 point) {
            throw new NotImplementedException();
        }

        public Vector2 LayerToCamera(Vector2 point) {
            throw new NotImplementedException();
        }

        public Vector2 LayerToLayer(Vector2 point, IRenderingLayer other) {
            throw new NotImplementedException();
        }

        public Vector2 WindowToLayer(Vector2 point) {
            throw new NotImplementedException();
        }

        public Vector2 LayerToWindow(Vector2 point) {
            throw new NotImplementedException();
        }

        public void WindowSizeChanged(Point oldSize, Point newSize) {
        }

        public void Begin() {
        }

        public void End() {
        }
    }

    [Event("fold:window.size_changed", EventFlushMode.End)]
    public struct WindowSizeChangedEvent {
        public Point OldSize;
        public Point NewSize;
        
        public WindowSizeChangedEvent(Point oldSize, Point newSize) {
            this.OldSize = oldSize;
            this.NewSize = newSize;
        }
    }
}
