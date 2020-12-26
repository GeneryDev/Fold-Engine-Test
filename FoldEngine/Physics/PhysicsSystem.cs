using FoldEngine.Components;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:physics", ProcessingCycles.Update)]
    public class PhysicsSystem : GameSystem {
        private ComponentIterator<Physics> _physicsObjects;

        public Vector2 Gravity = new Vector2(0, -900f);
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
        }

        public override void OnUpdate() {
            _physicsObjects.Reset();

            while(_physicsObjects.Next()) {
                ref Physics physics = ref _physicsObjects.GetComponent();
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();

                transform.Position += physics.Velocity * Time.DeltaTime;
                physics.Velocity += Gravity * Time.DeltaTime;
            }
        }
    }
}