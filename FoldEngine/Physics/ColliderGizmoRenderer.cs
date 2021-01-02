using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:gizmos.collider", ProcessingCycles.Update)]
    public class ColliderGizmoRenderer : GameSystem {
        private MultiComponentIterator _colliders;

        internal override void Initialize() {
            _colliders = Owner.Components.CreateMultiIterator(typeof(MeshCollider), typeof(BoxCollider))
                .SetGrouping(ComponentGrouping.Or);
        }

        public override void OnUpdate() {
            _colliders.Reset();
            while(_colliders.Next()) {
                ref Transform transform = ref _colliders.GetCoComponent<Transform>();
                
                if(_colliders.Has<BoxCollider>()) {
                    ref BoxCollider collider = ref _colliders.Get<BoxCollider>();

                    Vector2 a = transform.Apply(new Vector2(-collider.Width / 2, -collider.Height / 2));
                    Vector2 b = transform.Apply(new Vector2(-collider.Width / 2, collider.Height / 2));
                    Vector2 c = transform.Apply(new Vector2(collider.Width / 2, collider.Height / 2));
                    Vector2 d = transform.Apply(new Vector2(collider.Width / 2, -collider.Height / 2));
                    
                    Owner.DrawGizmo(a, b, Color.Blue);
                    Owner.DrawGizmo(b, c, Color.Blue);
                    Owner.DrawGizmo(c, d, Color.Blue);
                    Owner.DrawGizmo(d, a, Color.Blue);
                    
                    Owner.DrawGizmo(transform.Position, collider.GetReach(ref transform), Color.Lime);
                }
            }
        }
    }
}