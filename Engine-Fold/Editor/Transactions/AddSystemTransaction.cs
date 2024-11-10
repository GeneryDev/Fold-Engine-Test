using System;
using FoldEngine.Commands;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class AddSystemTransaction : Transaction<Scene>
{
    private readonly Type _type;

    public AddSystemTransaction(Type type)
    {
        _type = type;
    }

    public override bool Redo(Scene target)
    {
        target.Systems.Add(target.Core.RegistryUnit.Systems.CreateForType(_type));
        return true;
    }

    public override bool Undo(Scene target)
    {
        target.Systems.Remove(target.Systems.Get(_type));
        return true;
    }
}

public class RemoveSystemTransaction : Transaction<Scene>
{
    private Type _type;
    private GameSystem _system;
    private int _index;

    public RemoveSystemTransaction(Type type)
    {
        _type = type;
    }

    public override bool Redo(Scene target)
    {
        _system = target.Systems.Get(_type);
        _index = target.Systems.GetSystemIndex(_type);
        target.Systems.Remove(_system);
        return true;
    }

    public override bool Undo(Scene target)
    {
        target.Core.CommandQueue.Enqueue(new InsertSystemAtIndexCommand(target, _system, _index));
        return true;
    }
}