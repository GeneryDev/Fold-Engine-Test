using System;
using System.Reflection;
using FoldEngine.Commands;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Fields.Text;
using FoldEngine.Editor.Inspector;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Views;

public class EditorSceneSettingsView : EditorView
{
    public override void Render(IRenderingUnit renderer)
    {
        ContentPanel.MayScroll = true;

        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        var editingScene = editingTab.Scene;

        if (editingScene != null)
        {
            var resourceIdField = typeof(Resource).GetProperty(nameof(Resource.Identifier), BindingFlags.Public | BindingFlags.Instance);
            
            ContentPanel.Element<ComponentMemberLabel>().Text("Scene Identifier");
            
            ContentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth)
                .Value(editingScene?.Identifier ?? "")
                .EditedAction(ContentPanel.Environment.ActionPool.Claim<SetObjectFieldAction>()
                    .Object(editingScene).FieldInfo(resourceIdField));

            ContentPanel.Element<ComponentMemberBreak>();
        }

        if (editingTab.Scene != null && ContentPanel.Button("Save Scene", 14).IsPressed())
        {
            Core.CommandQueue.Enqueue(new SaveSceneCommand(editingTab.Scene, editingTab.Scene.Identifier ?? "__new_scene"));
        }
    }
}