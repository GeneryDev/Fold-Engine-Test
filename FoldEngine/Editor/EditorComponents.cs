using FoldEngine.Components;
using FoldEngine.Rendering;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public class EditorComponents {
        public Camera EditorCamera;
        public Transform EditorTransform;
        public Scene Scene;

        public EditorComponents(Scene scene) {
            Scene = scene;

            EditorTransform = Transform.InitializeComponent(scene, -1);
            EditorCamera = new Camera();
        }
    }
}