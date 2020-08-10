using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Systems;

using Sandbox.Components;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Systems
{
    [GameSystem("sandbox:health", ProcessingCycles.Update | ProcessingCycles.Render)]
    public class HealthSystem : GameSystem
    {
        private MultiComponentIterator LivingComponents;

        internal override void Initialize()
        {
            LivingComponents = CreateComponentIterator(typeof(Transform), typeof(Living)).SetGrouping(ComponentGrouping.Or);
        }

        public override void OnUpdate()
        {
            Console.WriteLine("HealthSystem update");
            //Owner.Components.DebugPrint<Transform>();
            //Owner.Components.DebugPrint<Living>();

            LivingComponents.Reset();

            while(LivingComponents.Next())
            {
                Console.WriteLine($"Entity {LivingComponents.GetEntityId()} has:");
                if (LivingComponents.Has<Transform>())
                {
                    Console.WriteLine(LivingComponents.Get<Transform>());
                };
                if (LivingComponents.Has<Living>())
                {
                    Console.WriteLine(LivingComponents.Get<Living>());
                };
            }
            /*
            foreach(var living in LivingComponents)
            {
                Console.WriteLine($"Entity {living.EntityId} has:");
                //Console.WriteLine(living.Get<Transform>());
                //Console.WriteLine(living.Get<Living>());
            }*/
            Console.WriteLine();
        }
    }
}
