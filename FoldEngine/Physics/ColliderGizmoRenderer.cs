using System;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
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
                Transform transform = _colliders.GetCoComponent<Transform>();
                Collider collider = _colliders.GetComponent();
                
                DrawColliderGizmos(Owner, transform, collider);
            }
        }

        public static void DrawColliderGizmos(Entity entity) {
            if(entity.HasComponent<Collider>()) DrawColliderGizmos(entity.Scene, entity.Transform, entity.GetComponent<Collider>());
        }

        public static void DrawColliderGizmos(Scene scene, Transform transform, Collider collider) {
            Color colliderColor = new Color(102, 226, 148);
            Color reachColor = new Color(236, 187, 136);
            switch(collider.Type) {
                case ColliderType.Box: {
                        
                    Vector2 a = transform.Apply(new Vector2(-collider.Width / 2, -collider.Height / 2));
                    Vector2 b = transform.Apply(new Vector2(-collider.Width / 2, collider.Height / 2));
                    Vector2 c = transform.Apply(new Vector2(collider.Width / 2, collider.Height / 2));
                    Vector2 d = transform.Apply(new Vector2(collider.Width / 2, -collider.Height / 2));
                    
                    scene.DrawGizmo(a, b, colliderColor);
                    scene.DrawGizmo(b, c, colliderColor);
                    scene.DrawGizmo(c, d, colliderColor);
                    scene.DrawGizmo(d, a, colliderColor);
                    
                    scene.DrawGizmo(transform.Position, collider.GetReach(ref transform), reachColor);
                    break;
                }
                case ColliderType.Mesh: {

                    Vector2 firstVertex = default;
                    Vector2 prevVertex = default;
                    bool first = true;
                    foreach(var localVertex in scene.Meshes.GetVerticesForMesh(collider.MeshIdentifier)) {
                        Vector2 vertex = transform.Apply(localVertex);
                        if(first) {
                            firstVertex = vertex;
                        } else {
                            scene.DrawGizmo(prevVertex, vertex, colliderColor);
                        }

                        first = false;
                        prevVertex = vertex;
                    }
                    scene.DrawGizmo(prevVertex, firstVertex, colliderColor);
                    
                    // Owner.DrawGizmo(transform.Position, collider.GetReach(ref transform), reachColor);
                    break;
                }
            }
        }
    }
}