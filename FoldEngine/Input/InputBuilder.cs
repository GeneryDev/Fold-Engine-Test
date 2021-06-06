using System;
using System.Collections.Generic;
using EntryProject.Util.JsonSerialization;

namespace FoldEngine.Input {
    public class InputBuilder {
        private InputDevices _devices;
        private JsonDeserializerObject _root;

        public InputBuilder(InputDevices devices, JsonDeserializerObject root) {
            _devices = devices;
            _root = root;
        }

        public List<Player> Build() {
            List<Player> players = new List<Player>();
            _root.GetArray("players")
                .Iterate(new JsonBranches().Add<JsonDeserializerObject>(o => {
                    players.Add(BuildPlayer(o));
                    return null;
                }));
            return players;
        }

        private Player BuildPlayer(JsonDeserializerObject rawPlayer) {
            Player player = new Player();

            rawPlayer.GetArray("control_schemes")
                .Iterate(new JsonBranches().Add<JsonDeserializerObject>(rawControlScheme =>
                    BuildControlScheme(rawControlScheme, player))
                );
                    
            return player;
        }

        private ControlScheme BuildControlScheme(JsonDeserializerObject rawControlScheme, Player player) {
            var controlScheme = new ControlScheme(rawControlScheme.Get<string>("name"));
            
            rawControlScheme.GetArray("devices").Iterate(new JsonBranches().Add<string>(s => {
                controlScheme.AddDevice(_devices[s]);
                return null;
            }));

            rawControlScheme.GetArray("actions")
                .Iterate(new JsonBranches().Add<JsonDeserializerObject>(rawAction =>
                    BuildAction(rawAction, controlScheme)));

            player.AddControlScheme(controlScheme);
            return controlScheme;
        }

        private IAction BuildAction(JsonDeserializerObject rawAction, ControlScheme controlScheme) {
            string identifier = rawAction.Get<string>("identifier");
            IInputDevice device = _devices[rawAction.Get<string>("device")];
            string type = rawAction.Get<string>("type");

            IAction action = null;
            
            switch(type) {
                case "button": {
                    string buttonName = rawAction.Get<string>("button");
                    
                    action = new ButtonAction(device.Get<ButtonInfo>(buttonName)) {
                        BufferTime = rawAction.Get<int>("buffer_time", true, 32)
                    };
                    break;
                }
                case "analog": {
                    if(rawAction.ContainsKey("analog")) {
                        int axis = rawAction.Get<int>("axis", true, 0);
                        var analogInfo = device.Get<IAnalogInfo>(rawAction.Get<string>("analog"));
                        int factor = 1;
                        if(rawAction.Get<bool>("invert", true)) {
                            factor = -1;
                        }
                        action = new AnalogAction(() => analogInfo[axis] * factor);
                        break;
                    } else if(rawAction.ContainsKey("opposite_buttons")) {
                        JsonDeserializerArray rawOppositeButtons = rawAction.GetArray("opposite_buttons");

                        ButtonInfo negative = device.Get<ButtonInfo>(rawOppositeButtons.Get<string>(0));
                        ButtonInfo positive = device.Get<ButtonInfo>(rawOppositeButtons.Get<string>(1));

                        int factor = 1;
                        if(rawAction.Get<bool>("invert", true)) {
                            factor = -1;
                        }
                        action = new AnalogAction(() => factor * ((negative.Pressed ? -1 : 0) + (positive.Pressed ? 1 : 0)));
                        break;
                    }

                    rawAction.Get<string>("analog"); //throw an exception for missing key
                    return null;
                }
                default: throw new ArgumentException($"Unknown action type {type}");
            }
            
            controlScheme.PutAction(identifier, action);
            return action;
        }
    }
}