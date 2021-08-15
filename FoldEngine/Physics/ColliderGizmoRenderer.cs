using System;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:gizmos.collider", ProcessingCycles.Render, runWhenPaused: true)]
    public class ColliderGizmoRenderer : GameSystem {
        private ComponentIterator<Collider> _colliders;

        internal override void Initialize() {
            _colliders = Owner.Components.CreateIterator<Collider>(IterationFlags.None);
        }

        public override void OnRender(IRenderingUnit renderer) {
            _colliders.Reset();
            while(_colliders.Next()) {
                ref Transform transform = ref _colliders.GetCoComponent<Transform>();

                ref Collider collider = ref _colliders.GetComponent();
                switch(collider.Type) {
                    case ColliderType.Box: {
                        
                        Vector2 a = transform.Apply(new Vector2(-collider.Width / 2, -collider.Height / 2));
                        Vector2 b = transform.Apply(new Vector2(-collider.Width / 2, collider.Height / 2));
                        Vector2 c = transform.Apply(new Vector2(collider.Width / 2, collider.Height / 2));
                        Vector2 d = transform.Apply(new Vector2(collider.Width / 2, -collider.Height / 2));
                    
                        Owner.DrawGizmo(a, b, Color.Blue);
                        Owner.DrawGizmo(b, c, Color.Blue);
                        Owner.DrawGizmo(c, d, Color.Blue);
                        Owner.DrawGizmo(d, a, Color.Blue);
                    
                        Owner.DrawGizmo(transform.Position, collider.GetReach(ref transform), Color.Lime);
                        break;
                    }
                    case ColliderType.Mesh: {

                        Vector2 firstVertex = default;
                        Vector2 prevVertex = default;
                        bool first = true;
                        foreach(var localVertex in Owner.Meshes.GetVerticesForMesh(collider.MeshIdentifier)) {
                            Vector2 vertex = transform.Apply(localVertex);
                            if(first) {
                                firstVertex = vertex;
                            } else {
                                Owner.DrawGizmo(prevVertex, vertex, Color.Blue);
                            }

                            first = false;
                            prevVertex = vertex;
                        }
                        Owner.DrawGizmo(prevVertex, firstVertex, Color.Blue);
                    
                        // Owner.DrawGizmo(transform.Position, collider.GetReach(ref transform), Color.Lime);
                        break;
                    }
                }
            }
        }
    }
}