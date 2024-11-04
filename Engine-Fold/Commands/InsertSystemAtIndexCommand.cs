using FoldEngine.Interfaces;
using FoldEngine.Systems;

namespace FoldEngine.Commands;

public class InsertSystemAtIndexCommand : ICommand
{
    private GameSystem _sys;
    private int _toIndex;

    public InsertSystemAtIndexCommand(GameSystem sys, int toIndex)
    {
        this._sys = sys;
        this._toIndex = toIndex;
    }

    public void Execute(IGameCore core)
    {
        core.ActiveScene.Systems.AddDirectly(_sys);
        core.ActiveScene.Systems.ChangeSystemOrder(_sys.GetType(), _toIndex);
    }
}