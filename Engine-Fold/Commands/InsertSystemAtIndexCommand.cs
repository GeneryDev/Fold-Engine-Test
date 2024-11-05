using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;

namespace FoldEngine.Commands;

public class InsertSystemAtIndexCommand : ICommand
{
    private Scene _scene;
    private GameSystem _sys;
    private int _toIndex;

    public InsertSystemAtIndexCommand(Scene scene, GameSystem sys, int toIndex)
    {
        this._scene = scene;
        this._sys = sys;
        this._toIndex = toIndex;
    }

    public void Execute(IGameCore core)
    {
        _scene.Systems.AddDirectly(_sys);
        _scene.Systems.ChangeSystemOrder(_sys.GetType(), _toIndex);
    }
}