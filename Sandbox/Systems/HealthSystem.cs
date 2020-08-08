using FoldEngine.Components;
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
        private MultiComponentView LivingComponents;

        internal override void Initialize()
        {
            LivingComponents = CreateComponentView(typeof(Transform), typeof(Living)).SetGrouping(ComponentGrouping.Or);
        }

        public override void OnUpdate()
        {
            
            Console.WriteLine("HealthSystem update");
            foreach(var living in LivingComponents)
            {
                Console.WriteLine($"Entity {living.EntityId} has:");
                Console.WriteLine(living.Get<Transform>());
                Console.WriteLine(living.Get<Living>());
            }
            Console.WriteLine();
        }
    }
}
