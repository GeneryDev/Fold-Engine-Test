using System;
using FoldEngine.Components;
using FoldEngine.Graphics.Atlas;
using FoldEngine.Systems;

namespace FoldEngine.Resources {
    [GameSystem(identifier: "fold:resource_preloader", ProcessingCycles.Update, runWhenPaused: true)]
    public class ResourcePreloader : GameSystem {
        
        private ComponentIterator<ResourceToPreload> _resourcesToLoad;
        
        public override void Initialize() {
            _resourcesToLoad = Scene.Components.CreateIterator<ResourceToPreload>(IterationFlags.None);
        }
        
        public override void OnUpdate() {
            _resourcesToLoad.Reset();
            while(_resourcesToLoad.Next()) {
                ref ResourceToPreload component = ref _resourcesToLoad.GetComponent();
                if(component.Type == null) continue;
                if(component.Identifier.Identifier == null) continue;
                ResourceAttribute resourceAttribute = Resource.AttributeOf(component.Type);
                if(resourceAttribute != null) {
                    if(component.Identifier.Identifier.StartsWith("#")) {
                        if(Scene.Core.ResourceIndex.Exists(resourceAttribute.ResourceType,
                            component.Identifier.Identifier)) {
                            PreloadGroup(resourceAttribute.ResourceType, component.Identifier.Identifier);
                        }
                    } else {
                        Scene.Resources.KeepLoaded(resourceAttribute.ResourceType, ref component.Identifier, preload: true);
                    }
                }
            }
        }

        private void PreloadGroup(Type type, string groupIdentifier) {
            foreach(string rawIdentifier in Scene.Core.ResourceIndex.GetIdentifiersInGroup(type, groupIdentifier)) {
                var identifier = new ResourceIdentifier(rawIdentifier);
                Scene.Resources.KeepLoaded(type, ref identifier, true);
            }
        }
    }
}