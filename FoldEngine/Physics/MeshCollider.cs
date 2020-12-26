using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics.mesh_collider")]
    [ComponentInitializer(typeof(MeshCollider), nameof(InitializeComponent))]
    public struct MeshCollider {
        public string MeshIdentifier;
        public Matrix Matrix;
        
        public static MeshCollider InitializeComponent(Scene scene, long entityId)
        {
            return new MeshCollider() { Matrix = Matrix.Identity };
        }
    }
}