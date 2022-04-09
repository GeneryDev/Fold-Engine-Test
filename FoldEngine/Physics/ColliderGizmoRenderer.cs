using System;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:gizmos.collider", ProcessingCycles.Render, runWhenPaused: true)]
    public class ColliderGizmoRenderer : GameSystem {
        private ComponentIterator<Collider> _colliders;

        public override void Initialize() {
            _colliders = Scene.Components.CreateIterator<Collider>(IterationFlags.None);
        }

        public override void OnRender(IRenderingUnit renderer) {
            _colliders.Reset();
            while(_colliders.Next()) {
                Transform transform = _colliders.GetCoComponent<Transform>();
                ref Collider collider = ref _colliders.GetComponent();
                
                DrawColliderGizmos(Scene, transform, ref collider);
            }
        }

        public static void DrawColliderGizmos(Entity entity) {
            if(entity.HasComponent<Collider>()) DrawColliderGizmos(entity.Scene, entity.Transform, ref entity.GetComponent<Collider>());
        }

        public static void DrawColliderGizmos(Scene scene, Transform transform, ref Collider collider) {
            Color colliderColor = new Color(102, 226, 148);
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
                    break;
                }
                case ColliderType.Mesh: {

                    Vector2 firstVertex = default;
                    Vector2 prevVertex = default;
                    bool first = true;
                    foreach(var localVertex in scene.Resources.Get<Mesh>(ref collider.MeshIdentifier, Mesh.Empty).GetVertices()) {
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