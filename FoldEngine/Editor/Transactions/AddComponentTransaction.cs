using System;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions {
    public class AddComponentTransaction : Transaction<EditorEnvironment> {

        private Type _type;
        private long _entityId;

        public AddComponentTransaction(Type type, long entityId) {
            _type = type;
            _entityId = entityId;
        }

        public override bool Redo(EditorEnvironment target) {
            if(target.Scene.Components.HasComponent<Transform>(_entityId) && !target.Scene.Components.HasComponent(_type, _entityId)) {
                target.Scene.Components.CreateComponent(_type, _entityId);
            } else {
                SceneEditor.ReportEditorGameConflict($"{nameof(AddComponentTransaction)}.{nameof(Redo)}");
            }

            return true;
        }

        public override bool Undo(EditorEnvironment target) {
            if(target.Scene.Components.HasComponent(_type, _entityId)) {
                target.Scene.Components.RemoveComponent(_type, _entityId);
            } else {
                SceneEditor.ReportEditorGameConflict($"{nameof(AddComponentTransaction)}.{nameof(Undo)}");
            }

            return true;
        }
    }
}