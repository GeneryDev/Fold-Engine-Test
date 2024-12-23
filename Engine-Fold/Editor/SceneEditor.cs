using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using FoldEngine.Commands;
using FoldEngine.Editor.Views;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Editor;

public static class SceneEditor
{
    public static void AttachEditor(Scene scene)
    {
        scene.CameraOverrides = new CameraOverrides(scene);

        scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Identifier + " - Scene Editor"));
        scene.Paused = true;

        scene.Systems.Add(new EditorBase());
    }

    public static void DetachEditor(Scene scene)
    {
        scene.CameraOverrides = null;

        scene.Core.CommandQueue.Enqueue(new SetWindowTitleCommand(scene.Identifier));
        scene.Paused = false;

        scene.Systems.Remove<EditorBase>();

        ResetViewport(scene.Core.RenderingUnit);
    }

    public static void ResetViewport(IRenderingUnit renderer)
    {
        renderer.Core.CommandQueue.Enqueue(new SetRootRendererGroupCommand(renderer.MainGroup));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ReportEditorGameConflict()
    {
        StackFrame caller = new StackTrace().GetFrame(1);
        MethodBase callerMethod = caller.GetMethod();
        Console.WriteLine(
            $"[WARN] Editor-Game conflict: Could not perform '{callerMethod.DeclaringType?.Name}.{callerMethod.Name}' due to scene modifications made outside the editor.");
    }
}