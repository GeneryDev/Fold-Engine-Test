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
        target.Scene.Systems.Add(target.Scene.Core.RegistryUnit.Systems.CreateForType(_type));
        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        target.Scene.Systems.Remove(target.Scene.Systems.Get(_type));
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
        _system = target.Scene.Systems.Get(_type);
        _index = target.Scene.Systems.GetSystemIndex(_type);
        target.Scene.Systems.Remove(_system);
        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        target.Scene.Core.CommandQueue.Enqueue(new InsertSystemAtIndexCommand(target.Scene, _system, _index));
        return true;
    }
}