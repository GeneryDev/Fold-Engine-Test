using System;
using System.Collections.Generic;
using FoldEngine.Events;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace Woofer {
    public class WooferRenderingUnit : IRenderingUnit {
        private Point _windowSize = new Point(1280, 720);

        public WooferRenderingUnit(WooferGameCore core) {
            Core = core;
        }

        public IGameCore Core { get; }

        public EffectManager Effects { get; set; }
        public FontManager Fonts { get; set; }

        public Point WindowSize {
            get => _windowSize;
            set {
                Point oldSize = _windowSize;
                _windowSize = value;
                if(Core.FoldGame != null)
                    if(value != oldSize) {
                        foreach(RenderGroup group in Groups.Values) group.WindowSizeChanged(oldSize, value);
                        Core.ActiveScene?.Events.Invoke(new WindowSizeChangedEvent(oldSize, value));
                    }
            }
        }

        public ITexture WhiteTexture { get; private set; }

        public Dictionary<string, RenderGroup> Groups { get; } = new Dictionary<string, RenderGroup>();

        public RenderGroup RootGroup { get; set; }
        public RenderGroup MainGroup { get; set; }

        public IRenderingLayer WindowLayer => MainGroup["screen"];
        public IRenderingLayer WorldLayer => MainGroup["world"];
        public IRenderingLayer GizmoLayer => MainGroup["gizmos"];

        public void Initialize() {
            Console.WriteLine("Initializing Rendering Unit");
            var mainSize = new Point(1280, 720);

            Groups["main"] = RootGroup = MainGroup = new RenderGroup(this) {
                Size = mainSize,
                ["world"] = new RenderingLayer(this) {
                    Name = "world", LayerSize = new Point(320, 180), Destination = new Rectangle(Point.Zero, mainSize),
                    LogicalSize = mainSize.ToVector2(),
                    FitToWindow = true,
                    Color = new Color(56, 56, 56)
                },
                ["gizmos"] = new RenderingLayer(this) {
                    Name = "gizmos", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                },
                ["screen"] = new RenderingLayer(this) {
                    Name = "screen", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                },
                ["hud"] = new RenderingLayer(this) {
                    Name = "hud", LayerSize = new Point(640, 360), Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                }
            };

            var fullSize = new Point(1920, 1040);


            Groups["editor"] = new RenderGroup(this) {
                Size = mainSize,
                ["editor_gui"] = new RenderingLayer(this) {
                    Name = "editor_gui", LayerSize = mainSize, Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                },
                ["editor_gui_game"] = new DependencyRenderingLayer(0),
                ["editor_gui_overlay"] = new RenderingLayer(this) {
                    Name = "editor_gui_overlay", LayerSize = mainSize,
                    Destination = new Rectangle(Point.Zero, mainSize),
                    FitToWindow = true,
                    LogicalSize = mainSize.ToVector2()
                }
            };
            Groups["editor"]
                .AddDependency(new RenderGroup.Dependency {
                    Group = new ResizableRenderGroup(MainGroup) {
                        Size = mainSize
                    },
                    Destination = new Rectangle(Point.Zero, mainSize)
                });

            WindowSize = RootGroup.Size;
            UpdateWindowSize();
        }

        public void LoadContent() {
            Texture.CreateConstants();
            WhiteTexture = Texture.White;

            Console.WriteLine("Loading Fonts");
            Fonts.LoadAll();
            Console.WriteLine("Fonts loaded");
        }

        public Rectangle GetGroupBounds(RenderGroup renderGroup) {
            Rectangle? bounds = RootGroup.GetBounds(renderGroup);
            if(bounds.HasValue) return bounds.Value;
            throw new ArgumentException(
                $"RenderGroup {renderGroup} is not present in the current RenderGroup hierarchy.");
        }

        public void UpdateWindowSize() {
            (Core.FoldGame.Graphics.PreferredBackBufferWidth, Core.FoldGame.Graphics.PreferredBackBufferHeight) =
                WindowSize;
            Core.FoldGame.Graphics.ApplyChanges();
        }
    }
}