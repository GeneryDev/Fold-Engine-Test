﻿using System;
using System.Collections.Generic;
using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public class Player
{
    private readonly List<ControlScheme> _controlSchemes = new List<ControlScheme>();
    public ControlScheme ActiveScheme { get; private set; }

    public T Get<T>(string identifier) where T : class, IAction
    {
        return ActiveScheme.Get<T>(identifier);
    }

    public void AddControlScheme(ControlScheme scheme)
    {
        _controlSchemes.Add(scheme);
    }

    private void SwitchControlScheme(ControlScheme controlScheme)
    {
        ActiveScheme = controlScheme;
        Console.WriteLine($"New input: {controlScheme.Name}");
        //TODO invoke event
    }

    public void Update(InputUnit inputUnit, int playerIndex)
    {
        if (ActiveScheme == null && _controlSchemes.Count >= 1)
        {
            ActiveScheme = _controlSchemes[0];
        }

        foreach (ControlScheme controlScheme in _controlSchemes)
        {
            if (controlScheme != ActiveScheme && controlScheme.IsBeingUsed)
            {
                SwitchControlScheme(controlScheme);
                break;
            }
        }

        // ActiveScheme?.Update(inputUnit, playerIndex);
    }

    public void HandleInputEvent(ref InputEvent evt, InputUnit inputUnit, int playerIndex)
    {
        // ActiveScheme?.HandleInputEvent(ref evt, inputUnit, playerIndex);
    }
}