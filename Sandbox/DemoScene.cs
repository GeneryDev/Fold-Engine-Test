using FoldEngine.Components;
using FoldEngine.Scenes;

using Microsoft.Xna.Framework;

using Sandbox.Components;
using Sandbox.Systems;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox
{
    internal class DemoScene : Scene
    {


        public override void Initialize()
        {
            var e0 = CreateEntity("Entity 0");
            var e1 = CreateEntity("Entity 1");
            var e2 = CreateEntity("Entity 2");


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

            e1.Transform.LocalPosition = new Vector2(10, 0);
            e0.Transform.Rotation = (float)Math.PI/2f;

            Console.WriteLine(e1.Transform.Position);

            e0.AddComponent<Living>();
            e1.AddComponent<Living>();

            e1.Transform.SetParent(e0);
            e2.Transform.SetParent(e0);


            ComponentReference<Transform>[] e0Children = e0.Transform.Children;

            Systems.Add<HealthSystem>();
            Console.WriteLine("initializing");


            Components.DebugPrint<Transform>();
            Components.DebugPrint<Living>();
        }
    }
}
