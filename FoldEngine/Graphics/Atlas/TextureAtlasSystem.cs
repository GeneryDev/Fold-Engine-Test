using System;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Systems;

namespace FoldEngine.Graphics.Atlas {
    [GameSystem(identifier: "fold:texture_atlas", ProcessingCycles.Update, runWhenPaused: true)]
    public class TextureAtlasSystem : GameSystem {
        
        private ComponentIterator<TextureAtlasComponent> _atlases;

        internal override void Initialize() {
            _atlases = Scene.Components.CreateIterator<TextureAtlasComponent>(IterationFlags.None);
        }

        public override void OnUpdate() {
            _atlases.Reset();
            while(_atlases.Next()) {
                long entityId = _atlases.GetEntityId();
                ref TextureAtlasComponent atlasComponent = ref _atlases.GetComponent();
                if(atlasComponent.Generated) continue;
                if(atlasComponent.Group == null || !atlasComponent.Group.StartsWith("#")) continue;
                if(Scene.Core.ResourceIndex.Exists<TextureR>(atlasComponent.Group)) {
                    Console.WriteLine("Group " + atlasComponent.Group + " exists!");
                    if(atlasComponent.Atlas == null) {
                        atlasComponent.Atlas = new TextureAtlas(atlasComponent.Group, Scene.Resources);
                        
                        foreach(string rawIdentifier in Scene.Core.ResourceIndex.GetIdentifiersInGroup<TextureR>(atlasComponent
                            .Group)) {
                            Console.WriteLine("Waiting for texture " + rawIdentifier);
                            atlasComponent.WaitingForLoad++;
                            var identifier = new ResourceIdentifier(rawIdentifier);
                            Scene.Resources.Load<TextureR>(ref identifier, texture => {
                                ref TextureAtlasComponent atlasComponent1 = ref Scene.Components.GetComponent<TextureAtlasComponent>(entityId);
                                atlasComponent1.WaitingForLoad--;
                                Console.WriteLine("Texture loaded: " + rawIdentifier);
                                atlasComponent1.Atlas.AddTexture(texture.Identifier, (TextureR) texture);
                            });
                        }
                    } else if(atlasComponent.WaitingForLoad == 0) {
                        Console.WriteLine("Generated atlas " + atlasComponent.Group);
                        atlasComponent.Atlas.Pack();
                        atlasComponent.Generated = true;
                        atlasComponent.Atlas = null;
                    }
                }
            }
        }
        
        
    }
}