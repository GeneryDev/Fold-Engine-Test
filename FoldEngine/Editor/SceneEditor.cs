using FoldEngine.Editor.Systems;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public static class SceneEditor {
        public static void AttachEditor(Scene scene) {
            // scene.Core.Paused = true;

            scene.Systems.Add<EditorRendering>();
            scene.Systems.Add<EditorMenu>();
        }
    }
}