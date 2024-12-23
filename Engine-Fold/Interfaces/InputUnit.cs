using System;
using FoldEngine.Input;
using FoldEngine.Util.JsonSerialization;

namespace FoldEngine.Interfaces;

public class InputUnit
{
    public IGameCore Core { get; }

    public InputDevices Devices;
    public Players Players = new Players();

    public InputUnit(IGameCore core)
    {
        Core = core;

        Devices = new InputDevices(this);
    }

    public void Update()
    {
        Devices.Update();
        Players.Update();
    }

    public void Setup(InputDefinition def)
    {
        foreach (Player player in new InputBuilder(Devices,
                         new JsonDeserializerRoot(def.Identifier, def.Root).AsObject())
                     .Build())
            Players.Add(player);
    }

    public void InvokeInputEvent(InputEvent evt)
    {
        // Console.WriteLine($"Input: {evt}");
        Core.Events.Invoke(evt);
    }
}