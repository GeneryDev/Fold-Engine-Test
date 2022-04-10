using FoldEngine.Components;
using FoldEngine.Serialization;

namespace FoldEngine.Graphics.Atlas {
    [Component("fold:texture_atlas")]
    public struct TextureAtlasComponent {
        public string Group;

        [DoNotSerialize]
        public bool Generated;

        [DoNotSerialize]
        public int WaitingForLoad;

        [DoNotSerialize]
        public TextureAtlas Atlas;
    }
}