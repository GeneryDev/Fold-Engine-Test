using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;
using Sandbox.Components;
using Sandbox.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using FoldEngine.Graphics;
using FoldEngine.Rendering;
using FoldEngine.Util;

namespace Sandbox {
    internal class DemoScene : Scene {
        public override void Initialize() {
            Entity e0 = CreateEntity("Entity 0");
            Entity e1 = CreateEntity("Entity 1");
            Entity e2 = CreateEntity("Entity 2");


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

            e1.Transform.LocalPosition = new Vector2(0, 0);
            // e0.Transform.Rotation = (float) Math.PI / 4f;

            Console.WriteLine(e1.Transform.Position);

            e0.AddComponent<Living>();
            e1.AddComponent<Living>();

            e1.Transform.SetParent(e0);


            ComponentReference<Transform>[] e0Children = e0.Transform.Children;

            Systems.Add<HealthSystem>();

            Systems.Add<LevelRenderer2D>();


            Components.DebugPrint<Transform>();
            Components.DebugPrint<Living>();
            
            
            
            
            
            Entity cam = CreateEntity("Camera");
            cam.AddComponent<Camera>().RenderToLayer = "screen";

            e0.AddComponent<LevelRenderable>();
            // e1.AddComponent<LevelRenderable>();

            ref MeshRenderable e1MR = ref e1.AddComponent<MeshRenderable>();
            e1MR.TextureIdentifier = "main:end_portal_colors";
            e1MR.MeshIdentifier = "circle";
            e1MR.Matrix = Matrix.CreateScale(128);
            // e1MR.Offset += Vector3.Up * 15;
            // e1MR.Scale *= 8;
            
            Meshes.Start("triangle")
                .Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0, 0))
                .Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0, 1))
                .Vertex(new Vector2(0.5f, -0.5f), new Vector2(1, 1))
                .End();

            Meshes.Start("circle");
            int segments = 180;
            Complex step = Complex.FromRotation((float) (Math.PI * 2 / segments));
            Complex current = new Complex(0.5f, 0);
            for(int i = 0; i < segments; i++) {
                Meshes.Vertex(Vector2.Zero, Vector2.One*0.5f);
                Meshes.Vertex(current, current + new Complex(0.5f, 0.5f));
                Meshes.Vertex(current * step, current * step + new Complex(0.5f, 0.5f));
                current *= step;
            }
            Meshes.End();
        }
    }
}
