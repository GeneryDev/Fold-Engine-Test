using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;
using Sandbox.Components;
using Sandbox.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using FoldEngine.Util;
using Woofer;

namespace Sandbox {
    internal class DemoScene : Scene {

        public override void Initialize() {
            BuildMeshes();
            
            Systems.Add<LevelRenderer2D>();
            
            Entity e0 = CreateEntity("Entity 0");
            Entity e1 = CreateEntity("Entity 1");
            Entity e2 = CreateEntity("Entity 2");

            Entity cam = CreateEntity("Main Camera");
            cam.Transform.LocalScale *= 1 / 32f;
            cam.AddComponent<Camera>().RenderToLayer = "screen";

            e1.AddComponent<LevelRenderable>();
            {
                ref MeshRenderable mr = ref e1.AddComponent<MeshRenderable>();
                mr.MeshIdentifier = "square";
                mr.TextureIdentifier = "beacon";
            }



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
            // // e1.Transform.SetParent(e0);
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


        }

        private void BuildMeshes() {
            
            Meshes.Start("square", MeshCollection.MeshInputType.Vertices)
                .Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0, 1))
                .Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0, 0))
                .Vertex(new Vector2(0.5f, 0.5f), new Vector2(1, 0))
                .Vertex(new Vector2(0.5f, -0.5f), new Vector2(1, 1))
                .End();
            
            Meshes.Start("circle", MeshCollection.MeshInputType.Vertices);
            const int segments = 90;
            Complex step = Complex.FromRotation((float) (Math.PI * 2 / segments));
            Complex current = new Complex(0.5f, 0);
            for(int i = 0; i < segments; i++) {
                Meshes.Vertex(current, current + new Complex(0.5f, 0.5f));
                current *= step;
            }
            Meshes.End();
            
            
            Meshes.Start("weird", MeshCollection.MeshInputType.Vertices)
                .Vertex(new Vector2(-1, 0), Vector2.Zero)
                .Vertex(new Vector2(-2, 1), Vector2.Zero)
                .Vertex(new Vector2(0, 2), Vector2.Zero)
                .Vertex(new Vector2(2, 0), Vector2.Zero)
                .Vertex(new Vector2(0, -2), Vector2.Zero)
                .Vertex(new Vector2(-2, -1), Vector2.Zero)
                .End();
        }

        public DemoScene(IGameController controller) : base(controller) { }
    }
}
