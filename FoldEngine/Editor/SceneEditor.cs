using FoldEngine.Commands;
using FoldEngine.Editor.Systems;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public static class SceneEditor {
        public static void AttachEditor(Scene scene) {
            scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Name + " - Scene Editor"));
            // scene.Core.Paused = true;

            scene.Systems.Add<EditorBase>();
            scene.Systems.Add<EditorMenu>();
            scene.Systems.Add<EditorEntitiesList>();
            scene.Systems.Add<EditorSystemsList>();
        }

        public static class Actions {
            public const int ChangeToMenu = 1;
            public const int Save = 2;
            public const int ExpandCollapseEntity = 3;
            public const int Test = 99;
        }
    }
}