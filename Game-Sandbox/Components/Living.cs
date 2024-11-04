using FoldEngine.Components;
using FoldEngine.Resources;

namespace Sandbox.Components {
    [Component("sandbox:living")]
    public struct Living {
        public int Health;
        public int MaxHealth;
        public bool Grounded;

        public ResourceIdentifier Texture;
        public ResourceIdentifier Resource;
        public ResourceIdentifier Resource2;

        public ResourceIdentifier JumpSound;

        public override string ToString() {
            return $"sandbox:living|{Health}/{MaxHealth}";
        }
    }
}