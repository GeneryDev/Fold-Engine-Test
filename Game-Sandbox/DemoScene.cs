using System;
using FoldEngine.Editor;
using FoldEngine.Graphics.Atlas;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Systems;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Physics.Systems;
using FoldEngine.Rendering;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Scenes.Prefabs;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Sandbox.Components;
using Sandbox.Systems;

namespace Sandbox;

internal class DemoScene : Scene
{
    public DemoScene(IGameCore core) : base(core, "demo_scene")
    {
        Initialize();
    }

    public void Initialize()
    {
        // BuildMeshes();

        Systems.Add<PrefabSystem>();
        Systems.Add<TextureAtlasSystem>();
        Systems.Add<SimplePhysicsSystem>();
        Systems.Add<LevelRenderer2D>();
        // Systems.Add<ColliderGizmoRenderer>();
        Systems.Add<DebugSystem>();

        Entity e0 = CreateEntity("Entity 0");
        Entity e1 = CreateEntity("Entity 1");
        Entity e2 = CreateEntity("Entity 2");
        Entity e3 = CreateEntity("Entity 3");
        Entity e4 = CreateEntity("Entity 4");

        Entity cam = CreateEntity("Main Camera");
        cam.Transform.LocalScale *= 1 / 64f;
        // cam.Transform.LocalPosition = Vector2.UnitY * -8;
        cam.Hierarchical.SetParent(e1);
        cam.AddComponent<Camera>();
        cam.GetComponent<Camera>().SnapPosition = 1 / 16f;

        {
            ref MeshRenderable mr = ref e1.AddComponent<MeshRenderable>();
            mr.MeshIdentifier = new ResourceIdentifier("square");
            mr.TextureIdentifier = new ResourceIdentifier("terrain/amethyst_block");
        }
        e1.Transform.Position += Vector2.UnitX * 0.3f;
        e1.Transform.Position += Vector2.UnitY * 64;
        e1.AddComponent<Physics>();
        e1.AddComponent<Collider>().SetBox(1, 1);
        e1.AddComponent<Living>();

        {
            ref MeshRenderable mr = ref e3.AddComponent<MeshRenderable>();
            mr.MeshIdentifier = new ResourceIdentifier("square");
            mr.TextureIdentifier = new ResourceIdentifier("terrain/amethyst_block");
        }
        e3.Transform.Position += Vector2.UnitY * 59.75f;
        e3.Transform.Position += Vector2.UnitX * -0.7f;
        e3.AddComponent<Physics>().Static = true;
        e3.AddComponent<Collider>().SetBox(1, 1);


        {
            ref MeshRenderable mr = ref e4.AddComponent<MeshRenderable>();
            mr.MeshIdentifier = new ResourceIdentifier("square");
            mr.TextureIdentifier = new ResourceIdentifier("terrain/amethyst_block");
        }
        e4.Transform.Position += Vector2.UnitY * 59.75f;
        e4.Transform.Position += Vector2.UnitX * 0.7f;
        e4.AddComponent<Physics>().Static = true;
        e4.AddComponent<Collider>().SetBox(1, 1);
        // e3.AddComponent<Living>();

        // {
        //     ref MeshRenderable mr = ref e0.AddComponent<MeshRenderable>();
        //     mr.MeshIdentifier = "square";
        //     mr.TextureIdentifier = "main:beacon";
        // }
        // e0.Transform.Position += Vector2.UnitY * 64;
        // e0.Transform.Position += Vector2.UnitX * -1.25f;
        // e0.AddComponent<Physics>();
        // e0.AddComponent<Collider>().SetMesh("circle");
        // // e0.AddComponent<Living>();


        {
            ref MeshRenderable mr = ref e2.AddComponent<MeshRenderable>();
            mr.MeshIdentifier = new ResourceIdentifier("square");
            mr.TextureIdentifier = new ResourceIdentifier("white");
        }
        e2.Name = "Platform";
        e2.Transform.Position += Vector2.UnitY * 57;
        e2.Transform.LocalScale = new Vector2(9, 4);
        e2.AddComponent<Physics>().Static = true;
        e2.AddComponent<Collider>().ThickFaces = true;

        
        
        Systems.Add<ControlLayoutSystem>();
        Systems.Add<StandardControlsSystem>();
        Systems.Add<ControlInterfaceSystem>();
        Systems.Add<ControlPopupSystem>();
        Systems.Add<TooltipSystem>();
        
        
        Entity viewport = CreateEntity("Viewport");
        viewport.AddComponent<Viewport>();
        
        Entity e5 = CreateEntity("Control");
        e5.Hierarchical.SetParent(viewport);
        ref var control = ref e5.AddComponent<Control>();
        control.Size = new Vector2(500, 500);
        control.RequestLayout = true;
        e5.AddComponent<BoxControl>().Color = Color.Salmon;

        Entity e6 = CreateEntity("Button");
        e6.AddComponent<Control>();
        ref var e6Anchors = ref e6.AddComponent<AnchoredControl>();
        e6Anchors = e6Anchors with
        {
            Anchor = new LRTB()
            {
                Left = 0.5f,
                Right = 0.5f,
                Top = 1.0f,
                Bottom = 1.0f
            },
            
            Offset = new LRTB()
            {
                Left = 0,
                Right = 0,
                Top = -70,
                Bottom = -48
            }
        };
        e6.AddComponent<ButtonControl>() = new ButtonControl()
        {
            Text = "Click me!",
            Icon = new ResourceIdentifier("editor/cog")
        };
        e6.AddComponent<SimpleTooltip>() = new SimpleTooltip()
        {
            Text = "Trust me!"
        };
        e6.Hierarchical.SetParent(e5);

        Entity container = CreateEntity("Container");
        container.AddComponent<Control>();
        container.AddComponent<FlowContainer>();
        container.Hierarchical.SetParent(e5);
        ref var containerAnchored = ref container.AddComponent<AnchoredControl>();
        containerAnchored = containerAnchored with
        {
            Anchor = AnchoredControl.Presets.FullRect
        };

        void AddBoxInContainer()
        {
            var color = new Color(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
            var box = CreateEntity("Box");
            ref var boxControl = ref box.AddComponent<Control>();
            boxControl.Size = boxControl.MinimumSize = new Vector2(Random.Shared.Next(40, 80), Random.Shared.Next(40, 80));
            box.AddComponent<BoxControl>().Color = color;
            box.Hierarchical.SetParent(container);
        }

        for (int i = 0; i < 10; i++)
        {
            AddBoxInContainer();
        }

        var labelEntity = CreateEntity("Label");
        labelEntity.Hierarchical.SetParent(e5);
        labelEntity.AddComponent<Control>() = new Control()
        {
            ZOrder = 5
        };
        labelEntity.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            Anchor = AnchoredControl.Presets.Center
        };
        labelEntity.AddComponent<LabelControl>() = new LabelControl()
        {
            Text = "top text\nhello world\nbottom text",
            Alignment = Alignment.Center
        };

        var labelBounds = CreateEntity("Label Bounds");
        labelBounds.Hierarchical.SetParent(labelEntity);
        labelBounds.AddComponent<Control>() = new Control
        {
            ZOrder = 4
        };
        labelBounds.AddComponent<AnchoredControl>() = new AnchoredControl
        {
            Anchor = AnchoredControl.Presets.FullRect
        };
        labelBounds.AddComponent<BoxControl>() = new BoxControl()
        {
            Color = new Color(0, 0, 0, 200)
        };

        /*e0.Transform.Position = new Vector2(1, 2);
        e1.Transform.Parent = e0.Transform;
        e1.Transform.Position = new Vector2(3, 2);

        Console.WriteLine(e0.Transform.Position);
        Console.WriteLine(e1.Transform.Position);

        e1.AddComponent<Living>();

        e0.Transform.LocalScale = new Vector2(0.5f, 0.5f);
        e1.Transform.LocalScale = new Vector2(0.5f, 0.5f);

        Console.WriteLine(e0.Transform.Position);
        Console.WriteLine(e1.Transform.Position);

        e1.Transform.LocalScale = Vector2.Zero;
        e1.Transform.LocalScale = Vector2.One * 0.5f;
        Console.WriteLine(e1.Transform.LocalScale);*/

        // // e0.Transform.Rotation = (float) Math.PI / 4f;
        //
        // Console.WriteLine(e1.Transform.Position);
        //
        // e0.AddComponent<Living>();
        // e1.AddComponent<Living>();
        //
        // // e1.Hierarchical.SetParent(e0);
        //
        //
        // ComponentReference<Transform>[] e0Children = e0.Transform.Children;
        //
        // Systems.Add<HealthSystem>();
        // Systems.Add<AdvancedPhysicsSystem>();
        //
        // Systems.Add<LevelRenderer2D>();
        // // Systems.Add<DebugRendering>();
        //
        //
        // Components.DebugPrint<Transform>();
        // Components.DebugPrint<Living>();
        //
        // // e0.Transform.LocalPosition += Vector2.UnitX * -80;
        // e0.Transform.LocalPosition += Vector2.UnitY * 256;
        // e2.Transform.LocalPosition += Vector2.UnitY * -128;
        //
        //
        //
        // Entity cam = CreateEntity("Camera");
        // cam.AddComponent<Camera>().RenderToLayer = "screen";
        //
        // e1.AddComponent<LevelRenderable>();
        // e1.Transform.LocalPosition += Vector2.UnitX * 400;
        // e1.Transform.LocalScale *= 128;
        // ref MeshRenderable e1MR = ref e1.AddComponent<MeshRenderable>();
        // e1MR.TextureIdentifier = "main:pixel.white";
        // e1MR.MeshIdentifier = "square";
        //
        // e1.AddComponent<Physics>().GravityMultiplier = 0;
        // e1.GetComponent<Physics>().Static = true;
        // e1.AddComponent<MeshCollider>().MeshIdentifier = "square";
        //
        // ref MeshRenderable e0MR = ref e0.AddComponent<MeshRenderable>();
        // e0MR.TextureIdentifier = "main:pixel.white";
        // e0MR.MeshIdentifier = "weird";
        // // e1MR.Matrix = Matrix.CreateScale(64);
        // e0MR.Color = Color.Black;
        // e0.Transform.LocalScale *= 32;
        //
        // e0.AddComponent<Physics>().GravityMultiplier = 1;
        // // e0.GetComponent<Physics>().AngularVelocity = (float) (Math.PI / 8);
        // e0.AddComponent<MeshCollider>().MeshIdentifier = "weird";

        // ref MeshRenderable e0MR = ref e1.AddComponent<MeshRenderable>();
        // e0MR.TextureIdentifier = "main:soul.start";
        // e0MR.MeshIdentifier = "weird";
        // e0MR.Matrix = Matrix.CreateScale(32);
        // e0MR.UVScale = new Vector2(-1, 1);
        // e0MR.UVOffset = new Vector2(1, 0);

        // ref MeshRenderable e2MR = ref e2.AddComponent<MeshRenderable>();
        // e2MR.MeshIdentifier = "weird";
        // e2MR.TextureIdentifier = "main:pixel.white";
        // e2.Transform.LocalScale *= 128;
        //
        // e2.AddComponent<Physics>().Static = true;
        // e2.AddComponent<MeshCollider>().MeshIdentifier = "weird";

        // Meshes.Start("triangle", MeshCollection.MeshInputType.Triangles)
        //     .Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0, 0))
        //     .Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0, 1))
        //     .Vertex(new Vector2(0.5f, -0.5f), new Vector2(1, 1))
        //     .End();

        // SceneEditor.AttachEditor(this);

        // SoundInstance music = Core.AudioUnit.CreateInstance("Audio/music");
        // music.Looping = true;
        // music.Play();
    }

    private void BuildMeshes()
    {
        Console.WriteLine("BUILDING MESHES");
        Resources.Create<Mesh>("square")
            .Start(Mesh.MeshInputType.Vertices)
            .Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0, 1))
            .Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0, 0))
            .Vertex(new Vector2(0.5f, 0.5f), new Vector2(1, 0))
            .Vertex(new Vector2(0.5f, -0.5f), new Vector2(1, 1))
            .End();

        Mesh circle = Resources.Create<Mesh>("circle").Start(Mesh.MeshInputType.Vertices);
        const int segments = 12;
        Complex step = Complex.FromRotation(-(float)(Math.PI * 2 / segments));
        var current = new Complex(0.5f, 0);
        for (int i = 0; i < segments; i++)
        {
            circle.Vertex(current, current + new Complex(0.5f, 0.5f));
            current *= step;
        }

        circle.End();


        Resources.Create<Mesh>("weird")
            .Start(Mesh.MeshInputType.Vertices)
            .Vertex(new Vector2(-1, 0), Vector2.Zero)
            .Vertex(new Vector2(-2, 1), Vector2.Zero)
            .Vertex(new Vector2(0, 2), Vector2.Zero)
            .Vertex(new Vector2(2, 0), Vector2.Zero)
            .Vertex(new Vector2(0, -2), Vector2.Zero)
            .Vertex(new Vector2(-2, -1), Vector2.Zero)
            .End();

        // Resources.Create<TestResource>("red").color = new Color(255, 0, 0);
        // Resources.Create<TestResource>("blue").color = new Color(0, 0, 255);
    }
}