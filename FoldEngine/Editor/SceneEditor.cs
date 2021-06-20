using FoldEngine.Commands;
using FoldEngine.Editor.Views;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public static class SceneEditor {
        public static void AttachEditor(Scene scene) {
            scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Name + " - Scene Editor"));
            // scene.Core.Paused = true;

            scene.Systems.Add<EditorBase>();
        }

        public static class Actions {
            public const int ChangeToMenu = 1;
            public const int Save = 2;
            public const int ExpandCollapseEntity = 3;
            public const int Test = 99;
        }
    }
}