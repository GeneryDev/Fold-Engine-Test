using System;
using System.Reflection;
using FoldEngine.Commands;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Fields.Text;
using FoldEngine.Editor.Inspector;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Views;

public class EditorDebugActionsView : EditorView
{
    public override void Render(IRenderingUnit renderer)
    {
        ContentPanel.MayScroll = true;

        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;

        if (ContentPanel.Button("Detach Editor (no undo!)", 14).IsPressed())
        {
            SceneEditor.DetachEditor(Scene);
            Console.WriteLine("Editor detached!");
        }

        if (ContentPanel.Button("Breakpoint", 14).IsPressed())
        {
            Console.WriteLine("Breakpoint!");
        }


        if (ContentPanel.Button("Save All Resources", 14).IsPressed())
        {
            editingTab.Scene?.Resources.SaveAll();
            Core.Resources.SaveAll();
            Console.WriteLine("Save resources!");
        }

        if (ContentPanel.Button("Reload Resources", 14).IsPressed()) Core.ResourceIndex.Update();
        if (ContentPanel.Button("Invoke GC", 14).IsPressed()) GC.Collect(GC.MaxGeneration);
        if (!editorBase.InspectSelf)
        {
            if (ContentPanel.Button("Start Inspecting Editor Scene", 14).IsPressed()) editorBase.InspectSelf = true;
        }
        else
        {
            if (ContentPanel.Button("Stop Inspecting Editor Scene", 14).IsPressed()) editorBase.InspectSelf = false;
        }
    }
}