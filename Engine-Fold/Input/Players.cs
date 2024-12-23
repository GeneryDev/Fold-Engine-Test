using System.Collections;
using System.Collections.Generic;
using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public class Players : IEnumerable<Player>
{
    public InputUnit InputUnit;
    private readonly List<Player> _players = new List<Player>();

    public Players(InputUnit inputUnit)
    {
        InputUnit = inputUnit;
    }

    public Player this[int index]
    {
        get => _players[index];
        set => _players[index] = value;
    }

    public IEnumerator<Player> GetEnumerator()
    {
        return _players.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Update()
    {
        for (var index = 0; index < _players.Count; index++)
        {
            var player = _players[index];
            player.Update(InputUnit, index);
        }
    }

    public void HandleInputEvent(ref InputEvent evt)
    {
        for (var index = 0; index < _players.Count; index++)
        {
            var player = _players[index];
            player.HandleInputEvent(ref evt, InputUnit, index);
        }
    }

    public void Add(Player player)
    {
        _players.Add(player);
    }
}