using FoldEngine.Components;
using FoldEngine.Rendering;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public class EditorComponents {
        public Scene Scene;
        public Transform EditorTransform;
        public Camera EditorCamera;

        public EditorComponents(Scene scene) {
            Scene = scene;
            
            EditorTransform = Transform.InitializeComponent(scene, -1);
            EditorCamera = new Camera();
        }
    }
}