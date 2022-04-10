using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions {
    public class DeleteEntityTransaction : Transaction<EditorEnvironment> {
        private readonly long _entityId;
        private long _parentEntityId;
        private byte[] _serializedData;

        public DeleteEntityTransaction(long entityId) {
            _entityId = entityId;
        }

        public override bool Redo(EditorEnvironment target) {
            if(_serializedData == null) {
                var stream = new MemoryStream();

                var saveOp = new SaveOperation(stream);
                saveOp.Options.Set(SerializeOnlyEntities.Instance, new List<long> {_entityId});

                target.Scene.Serialize(saveOp);

                saveOp.Close();
                _serializedData = stream.GetBuffer();
                saveOp.Dispose();
            }

            if(target.Scene.Components.HasComponent<Transform>(_entityId)) {
                ref Transform transform = ref target.Scene.Components.GetComponent<Transform>(_entityId);
                _parentEntityId = transform.ParentId;

                if(_parentEntityId != -1 && target.Scene.Components.HasComponent<Transform>(_parentEntityId))
                    target.Scene.Components.GetComponent<Transform>(_parentEntityId).RemoveChild(_entityId);
                target.Scene.DeleteEntity(_entityId, true);
            } else {
                SceneEditor.ReportEditorGameConflict();
            }


            return true;
        }

        public override bool Undo(EditorEnvironment target) {
            if(_serializedData == null) throw new InvalidOperationException("Cannot call Undo before Redo");

            if(target.Scene.Reclaim(_entityId)) {
                var loadOp = new LoadOperation(new MemoryStream(_serializedData));

                target.Scene.Deserialize(loadOp);

                loadOp.Close();
                loadOp.Dispose();

                if(_parentEntityId != -1)
                    target.Scene.Components.GetComponent<Transform>(_entityId).SetParent(_parentEntityId);
            }

            return true;
        }
    }
}