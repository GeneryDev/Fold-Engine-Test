using System;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Interfaces;

namespace FoldEngine.Editor.Views {
    public class EditorInspectorView : EditorView {
        public override string Icon => "editor:info";
        public override string Name => "Inspector";

        private long _id = -1;
        
        public override void Render(IRenderingUnit renderer) {
            if(_id != -1) {
                ContentPanel.Label(Scene.Components.GetComponent<EntityName>(_id).Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cube"]);
                ContentPanel.Label($"ID: {_id}", 1).TextAlignment(-1);
                ContentPanel.Spacing(12);

                foreach(ComponentSet set in Scene.Components.Sets.Values) {
                    if(set.ComponentType == typeof(EntityName)) continue;
                    if(set.Has((int) _id)) {
                        ContentPanel.Separator();
                        ContentPanel.Label(set.ComponentType.Name, 2).TextAlignment(-1);
                        
                        foreach(FieldInfo fieldInfo in set.ComponentType.GetFields()) {
                            object value = fieldInfo.GetValue(set.BoxedGet((int)_id));
                            ContentPanel.Label($"{fieldInfo.Name}: {value}", 1).TextAlignment(-1);
                        }
                    }
                }
            }
            // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        }

        public void SetEntity(long id) {
            _id = id;
        }
    }
}