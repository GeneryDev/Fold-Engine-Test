using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions {
    public class SetEntityTransformTransaction : Transaction<EditorEnvironment> {
        private long _entityId;
        private Transform _before;
        private Transform _after;

        public void UpdateAfter(Transform after) {
            _after = after;
        }

        public SetEntityTransformTransaction(Transform before) {
            _before = _after = before;
            _entityId = before.EntityId;
        }

        public override bool Redo(EditorEnvironment target) {
            ref Transform targetTransform = ref target.Scene.Components.GetComponent<Transform>(_entityId);

            bool any = false;
            
            if(_before.LocalPosition != _after.LocalPosition) {
                targetTransform.Position = _after.LocalPosition;
                any = true;
            }
            if(_before.LocalRotation != _after.LocalRotation) {
                targetTransform.Rotation = _after.LocalRotation;
                any = true;
            }
            if(_before.LocalScale != _after.LocalScale) {
                targetTransform.LocalScale = _after.LocalScale;
                any = true;
            }

            return any;
        }

        public override bool Undo(EditorEnvironment target) {
            ref Transform targetTransform = ref target.Scene.Components.GetComponent<Transform>(_entityId);

            bool any = false;
            
            if(_before.LocalPosition != _after.LocalPosition) {
                targetTransform.Position = _before.LocalPosition;
                any = true;
            }
            if(_before.LocalRotation != _after.LocalRotation) {
                targetTransform.Rotation = _before.LocalRotation;
                any = true;
            }
            if(_before.LocalScale != _after.LocalScale) {
                targetTransform.LocalScale = _before.LocalScale;
                any = true;
            }

            return any;
        }

        public override bool RedoOnInsert(EditorEnvironment target) {
            return true;
        }
    }
}