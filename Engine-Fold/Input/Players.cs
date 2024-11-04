using System.Collections;
using System.Collections.Generic;

namespace FoldEngine.Input;

public class Players : IEnumerable<Player>
{
    private readonly List<Player> _players = new List<Player>();

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
        foreach (Player player in this) player.Update();
    }

    public void Add(Player player)
    {
        _players.Add(player);
    }
}