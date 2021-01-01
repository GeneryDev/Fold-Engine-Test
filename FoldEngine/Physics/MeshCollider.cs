using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics.mesh_collider")]
    public struct MeshCollider {
        public string MeshIdentifier;
    }
}