using System;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Commands;

public class ChangeSystemOrderCommand : ICommand
{
    private Scene _scene;
    private Type _sysType;
    private int _toIndex;

    public ChangeSystemOrderCommand(Scene scene, Type sysType, int toIndex)
    {
        this._scene = scene;
        this._sysType = sysType;
        this._toIndex = toIndex;
    }

    public void Execute(IGameCore core)
    {
        _scene.Systems.ChangeSystemOrder(_sysType, _toIndex);
    }
}