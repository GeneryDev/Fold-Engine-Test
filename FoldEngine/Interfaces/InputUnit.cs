﻿using EntryProject.Util.JsonSerialization;
using FoldEngine.Input;

namespace FoldEngine.Interfaces {
    public class InputUnit {
        public InputDevices Devices = new InputDevices();
        public Players Players = new Players();

        public void Update() {
            Devices.Update();
            Players.Update();
        }

        public void Setup(InputDefinition def) {
            foreach(Player player in new InputBuilder(Devices,
                    new JsonDeserializerRoot(def.Identifier, def.Root).AsObject())
                .Build())
                Players.Add(player);
        }
    }
}