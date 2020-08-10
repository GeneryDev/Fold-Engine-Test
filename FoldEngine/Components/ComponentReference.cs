using FoldEngine.Scenes;

using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components
{
    public class ComponentReference<T> where T : struct
    {
        Scene Scene;
        long EntityId;

        public ComponentReference(Scene scene, long entityId)
        {
            Scene = scene;
            EntityId = entityId;
        }

        public ref T Get()
        {
            return ref Scene.Components.GetComponent<T>(EntityId);
        }
    }
}
