using System;
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

        public static void ReportEditorGameConflict(string actionName) {
            Console.WriteLine($"[WARN] Editor-Game conflict: Could not perform '{actionName}' due to scene modifications made outside the editor.");
        }
    }
}