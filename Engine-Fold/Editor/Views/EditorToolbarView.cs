using System;
using System.IO;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Systems;
using FoldEngine.Editor.Tools;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Editor.Views;

public class EditorToolbarView : EditorView
{
    public EditorToolbarView()
    {
        new ResourceIdentifier("editor/cog");
    }

    public virtual string Name => "Toolbar";

    public override void Render(IRenderingUnit renderer)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        ref var editingTab = ref editorBase.CurrentSceneTab;
        
        var toolSystem = Scene.Systems.Get<EditorToolSystem>();

        if (toolSystem != null)
        {
            foreach (EditorTool tool in toolSystem.Tools)
                if (ContentPanel.Element<ToolbarButton>()
                    .Down(tool == toolSystem.ActiveTool)
                    .Text("")
                    .FontSize(14)
                    .Icon(EditorResources.Get<Texture>(ref tool.Icon))
                    .IsPressed())
                    toolSystem.SelectedTool = tool;
        }

        // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        // ContentPanel.Element<ToolbarButton>().Text("Save").FontSize(14).Icon(renderer.Textures["editor:cog"]);
        // ContentPanel.Separator();

        if (editingTab.Scene != ContentPanel.Environment.Scene)
        {
            if (ContentPanel.Element<ToolbarButton>()
                .Text("")
                .FontSize(14)
                .Icon(EditorResources.Get<Texture>(ref EditorIcons.Play))
                .IsPressed())
            {
                if (editingTab.StoredSceneData == null)
                {
                    Play(ContentPanel.Environment as EditorEnvironment, ref editingTab);
                }
                else
                {
                    Stop(ContentPanel.Environment as EditorEnvironment, ref editingTab);
                    Core.AudioUnit.StopAll();
                    GC.Collect(GC.MaxGeneration);
                }
            }

            if (editingTab.Scene != null && editingTab.Playing
                && ContentPanel.Element<ToolbarButton>()
                .Down(editingTab.Scene.Paused)
                .Text("")
                .FontSize(14)
                .Icon(EditorResources.Get<Texture>(ref EditorIcons.Pause))
                .IsPressed())
                editingTab.Scene.Paused = !editingTab.Scene.Paused;
        }
        // ContentPanel.Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
        // ContentPanel.Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
        // ContentPanel.Button("Edit Save Data").Action(SceneEditor.Actions.Test, 0);
        // ContentPanel.Element<ToolbarButton>().Text("Quit").FontSize(14);
    }

    private void Play(EditorEnvironment environment, ref EditorSceneTab editingSceneTab)
    {
        var stream = new MemoryStream();

        var saveOp = new BinarySaveOperation(stream);
        saveOp.Options.Set(SerializeTempResources.Instance, true);
        Scene.Systems.Get<EditorBase>().CurrentSceneTab.Scene.Serialize(saveOp);

        saveOp.Close();
        editingSceneTab.StoredSceneData = stream.GetBuffer();
        saveOp.Dispose();
        SetScenePlaying(environment, ref editingSceneTab, true);
    }

    private void Stop(EditorEnvironment environment, ref EditorSceneTab editingSceneTab)
    {
        var loadOp = new BinaryLoadOperation(new MemoryStream(editingSceneTab.StoredSceneData));

        loadOp.Options.Set(DeserializeClearScene.Instance, true);

        var editingScene = Scene.Systems.Get<EditorBase>().CurrentSceneTab.Scene;
        editingScene.Deserialize(loadOp);
        editingScene.Flush();

        loadOp.Close();
        loadOp.Dispose();
        editingSceneTab.StoredSceneData = null;
        SetScenePlaying(environment, ref editingSceneTab, false);
    }

    private void SetScenePlaying(EditorEnvironment environment, ref EditorSceneTab editingSceneTab, bool playing)
    {
        editingSceneTab.Playing = playing;

        ref var subScene = ref environment.Scene.Systems.Get<EditorBase>().CurrentSubScene;
        subScene.Update = subScene.ProcessInputs = playing;
        subScene.Render = true;
    }
}