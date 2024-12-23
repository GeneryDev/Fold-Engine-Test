using System;
using System.Collections.Generic;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Input;

public class Keyboard : IInputDevice
{
    private Keys[] _allKeys;
    
    private readonly List<ButtonInfo> _trackedKeyInfo = new List<ButtonInfo>();
    private readonly List<Keys> _trackedKeys = new List<Keys>();
    
    private readonly List<TextInputEventArgs> _typedEventQueue = new();

    public ButtonInfo this[Keys key]
    {
        get
        {
            int existingIndex = _trackedKeys.IndexOf(key);
            if (existingIndex != -1)
            {
                return _trackedKeyInfo[existingIndex];
            }

            var newInfo = new ButtonInfo(() =>
                Microsoft.Xna.Framework.Input.Keyboard.GetState()[key] == KeyState.Down);
            _trackedKeyInfo.Add(newInfo);
            _trackedKeys.Add(key);
            return newInfo;
        }
    }

    public bool ControlDown => this[Keys.LeftControl].Down || this[Keys.RightControl].Down;
    public bool ShiftDown => this[Keys.LeftShift].Down || this[Keys.RightShift].Down;
    public bool AltDown => this[Keys.LeftAlt].Down || this[Keys.RightAlt].Down;

    public bool IsBeingUsed => Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys().Length > 0;
    
    private KeyboardState _prevState;

    public void Update(InputUnit inputUnit)
    {
        if (!inputUnit.Core.FoldGame.IsActive) return;
        
        if (_allKeys == null)
        {
            inputUnit.Core.FoldGame.Window.TextInput += OnTextInput;
        }
        _allKeys ??= Enum.GetValues<Keys>();
        
        
        var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        
        foreach (ButtonInfo info in _trackedKeyInfo) info.Update();
        foreach (var key in _allKeys)
        {
            bool wasDown = _prevState.IsKeyDown(key);
            bool down = keyboardState.IsKeyDown(key);

            bool typed = WasTyped(key, out var typedEvt);

            if (wasDown != down || typed)
            {
                inputUnit.InvokeInputEvent(new InputEventKey(-1)
                {
                    Key = key,
                    Modifiers = GetKeyModifiers(keyboardState),
                    Pressed = down,
                    Character = typedEvt.Character,
                    IsEcho = typed && !wasDown
                }.UnderlyingEvent);
            }
        }

        _prevState = keyboardState;
        _typedEventQueue.Clear();
    }

    private bool WasTyped(Keys key, out TextInputEventArgs typedEvt)
    {
        for (var typedIndex = 0; typedIndex < _typedEventQueue.Count; typedIndex++)
        {
            var evt = _typedEventQueue[typedIndex];
            if (evt.Key != key) continue;
            
            typedEvt = evt;
            _typedEventQueue.RemoveAt(typedIndex);
            return true;
        }

        typedEvt = default;
        return false;
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        _typedEventQueue.Add(e);
    }

    public KeyModifiers GetKeyModifiers() => GetKeyModifiers(_prevState);
    
    public bool IsKeyDown(Keys keys)
    {
        return _prevState.IsKeyDown(keys);
    }

    private static KeyModifiers GetKeyModifiers(KeyboardState state)
    {
        return (state[Keys.LeftControl] == KeyState.Down || state[Keys.RightControl] == KeyState.Down
                   ? KeyModifiers.Control
                   : KeyModifiers.None)
               | (state[Keys.LeftShift] == KeyState.Down || state[Keys.RightShift] == KeyState.Down
                   ? KeyModifiers.Shift
                   : KeyModifiers.None)
               | (state[Keys.LeftAlt] == KeyState.Down || state[Keys.RightAlt] == KeyState.Down
                   ? KeyModifiers.Alt
                   : KeyModifiers.None)
               | (state[Keys.LeftWindows] == KeyState.Down || state[Keys.RightWindows] == KeyState.Down
                   ? KeyModifiers.Meta
                   : KeyModifiers.None)
            ;
    }

    public T Get<T>(string name) where T : IInputInfo
    {
        if (typeof(T) == typeof(ButtonInfo))
        {
            Enum.TryParse(name, true, out Keys k);
            if (k != Keys.None) return (T)(IInputInfo)this[k];
        }

        throw new ArgumentException(name);
    }
}