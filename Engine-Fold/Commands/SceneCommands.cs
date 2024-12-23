using System;
using System.Collections.Generic;
using FoldEngine.Editor;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace FoldEngine.Commands;

public class SaveSceneCommand : ICommand
{
    private Scene _scene;
    private string _identifier;

    public SaveSceneCommand(Scene scene, string identifier)
    {
        this._scene = scene;
        this._identifier = identifier;
    }

    public void Execute(IGameCore core)
    {
        string savePath = core.RegistryUnit.Resources.AttributeOf(_scene.GetType()).CreateResourcePath(_identifier);
        _scene.Save(savePath);

        core.ResourceIndex.Update();
    }
}

public class LoadSceneCommand : ICommand
{
    public ResourceIdentifier Identifier;
    public bool AttachEditor;

    public LoadSceneCommand(string identifier, bool attachEditor = false)
    {
        Identifier = new ResourceIdentifier(identifier);
        AttachEditor = attachEditor;
    }

    public void Execute(IGameCore core)
    {
        core.Resources.Load<Scene>(ref Identifier, s =>
        {
            Console.WriteLine("Successfully loaded!");
            core.ActiveScene = (Scene)s;
            core.Resources.Detach(s);
            if (AttachEditor)
            {
                SceneEditor.AttachEditor((Scene)s);
            }
            else
            {
                SceneEditor.ResetViewport(core.RenderingUnit);
            }
        });
        // var loadOp = new LoadOperation(SourcePath);
        //
        // loadOp.Options.Set(DeserializeClearScene.Instance, true);
        // loadOp.Options.Set(DeserializeRemapIds.Instance, new EntityIdRemapper(core.ActiveScene));
        //
        // core.ActiveScene.Deserialize(loadOp);
        //
        // loadOp.Close();
        // loadOp.Dispose();
    }
}