using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using FoldEngine.Commands;
using FoldEngine.Scenes;

namespace FoldEngine.Editor {
    public static class SceneEditor {
        public static void AttachEditor(Scene scene) {
            scene.EditorComponents = new EditorComponents(scene);

            scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Name + " - Scene Editor"));
            scene.Paused = true;

            scene.Systems.Add<EditorBase>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReportEditorGameConflict() {
            StackFrame caller = new StackTrace().GetFrame(1);
            MethodBase callerMethod = caller.GetMethod();
            Console.WriteLine(
                $"[WARN] Editor-Game conflict: Could not perform '{callerMethod.DeclaringType?.Name}.{callerMethod.Name}' due to scene modifications made outside the editor.");
        }
    }
}