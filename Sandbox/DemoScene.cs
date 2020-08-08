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
            var e0 = CreateEntity();
            var e1 = CreateEntity();


            e0.Transform.Position = new Vector2(1, 2);
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
            Console.WriteLine(e1.Transform.LocalScale);

            Systems.Add<HealthSystem>();
            Console.WriteLine("initializing");
        }
    }
}
