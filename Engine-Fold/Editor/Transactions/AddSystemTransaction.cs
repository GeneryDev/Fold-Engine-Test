using System;
using FoldEngine.Commands;
using FoldEngine.Editor.Gui;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class AddSystemTransaction : Transaction<EditorEnvironment>
{
    private readonly Type _type;

    public AddSystemTransaction(Type type)
    {
        _type = type;
    }

    public override bool Redo(EditorEnvironment target)
    {
        target.EditingScene.Systems.Add(target.Core.RegistryUnit.Systems.CreateForType(_type));
        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        target.EditingScene.Systems.Remove(target.EditingScene.Systems.Get(_type));
        return true;
    }
}

public class RemoveSystemTransaction : Transaction<EditorEnvironment>
{
    private Type _type;
    private GameSystem _system;
    private int _index;

    public RemoveSystemTransaction(Type type)
    {
        _type = type;
    }

    public override bool Redo(EditorEnvironment target)
    {
        _system = target.EditingScene.Systems.Get(_type);
        _index = target.EditingScene.Systems.GetSystemIndex(_type);
        target.EditingScene.Systems.Remove(_system);
        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        target.EditingScene.Core.CommandQueue.Enqueue(new InsertSystemAtIndexCommand(target.EditingScene, _system, _index));
        return true;
    }
}