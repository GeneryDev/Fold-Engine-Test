using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using FoldEngine.Commands;
using FoldEngine.Editor.Views;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor {
    public static class SceneEditor {
        public static void AttachEditor(Scene scene) {
            scene.EditorComponents = new EditorComponents(scene);

            scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Name + " - Scene Editor"));
            scene.Paused = true;

            scene.Systems.Add<EditorBase>();
            EditorToolbarView.NewSceneLoaded();
        }
        public static void DetachEditor(Scene scene) {
            scene.EditorComponents = null;

            scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Name));
            scene.Paused = false;

            scene.Systems.Remove<EditorBase>();

            ResetViewport(scene.Core.RenderingUnit);
        }

        public static void ResetViewport(IRenderingUnit renderer) {
            renderer.Groups["editor"].Dependencies[0].Group.Size = renderer.WindowSize;
            renderer.Groups["editor"].Dependencies[0].Destination = new Rectangle(Point.Zero, renderer.WindowSize);
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