using FoldEngine.Scenes;

using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components {
    public class ComponentReference<T> where T : struct {
        Scene _scene;
        long _entityId;

        public ComponentReference(Scene scene, long entityId) {
            _scene = scene;
            _entityId = entityId;
        }

        public ref T Get() {
            return ref _scene.Components.GetComponent<T>(_entityId);
        }

        public bool Has() {
            return _scene.Components.HasComponent<T>(_entityId);
        }
    }
}
