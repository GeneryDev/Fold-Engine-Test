using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Rendering {
    [Component("fold:mesh_renderable")]
    [ComponentInitializer(typeof(MeshRenderable), nameof(InitializeComponent))]
    public struct MeshRenderable {
        public string TextureIdentifier;
        public string MeshIdentifier;
        public Matrix Matrix;
        public Vector2 UVOffset;
        public Vector2 UVScale;
        
        /// <summary>
        /// Returns an initialized mesh renderable component with all its correct default values.
        /// </summary>
        /// <param name="scene">The scene this component is being created in</param>
        /// <param name="entityId">The ID of the entity this component is being created for</param>
        /// <returns>An initialized component with all its correct default values.</returns>
        public static MeshRenderable InitializeComponent(Scene scene, long entityId)
        {
            return new MeshRenderable() { Matrix = Matrix.Identity, UVScale = Vector2.One};
        }
    }
}