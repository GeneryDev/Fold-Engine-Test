using EntryProject.Util.JsonSerialization;
using FoldEngine.Input;

namespace FoldEngine.Interfaces {
    public class IInputUnit {
        public InputDevices Devices = new InputDevices();
        public Players Players = new Players();

        public void Update() {
            Devices.Update();
            Players.Update();
        }

        public void Setup(string configPath) {
            foreach(Player player in new InputBuilder(Devices, JsonDeserializerRoot.NewFromFile(configPath).AsObject())
                .Build()) {
                Players.Add(player);
            }
        }
    }
}