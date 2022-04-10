using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions {
    public class AddComponentTransaction : Transaction<EditorEnvironment> {
        private readonly long _entityId;

        private readonly Type _type;

        public AddComponentTransaction(Type type, long entityId) {
            _type = type;
            _entityId = entityId;
        }

        public override bool Redo(EditorEnvironment target) {
            if(target.Scene.Components.HasComponent<Transform>(_entityId)
               && !target.Scene.Components.HasComponent(_type, _entityId))
                target.Scene.Components.CreateComponent(_type, _entityId);
            else
                SceneEditor.ReportEditorGameConflict();

            return true;
        }

        public override bool Undo(EditorEnvironment target) {
            if(target.Scene.Components.HasComponent(_type, _entityId))
                target.Scene.Components.RemoveComponent(_type, _entityId);
            else
                SceneEditor.ReportEditorGameConflict();

            return true;
        }
    }

    public class RemoveComponentTransaction : Transaction<EditorEnvironment> {
        private readonly long _entityId;

        private byte[] _serializedData;

        private readonly Type _type;

        public RemoveComponentTransaction(Type type, long entityId) {
            _type = type;
            _entityId = entityId;
        }

        public override bool Redo(EditorEnvironment target) {
            if(_serializedData == null) {
                var stream = new MemoryStream();

                var saveOp = new SaveOperation(stream);
                saveOp.Options.Set(SerializeOnlyEntities.Instance, new List<long> {_entityId});
                saveOp.Options.Set(SerializeOnlyComponents.Instance, new List<Type> {_type});

                target.Scene.Serialize(saveOp);

                saveOp.Close();
                _serializedData = stream.GetBuffer();
                saveOp.Dispose();
            }

            if(target.Scene.Components.HasComponent(_type, _entityId))
                target.Scene.Components.RemoveComponent(_type, _entityId);
            else
                SceneEditor.ReportEditorGameConflict();

            return true;
        }

        public override bool Undo(EditorEnvironment target) {
            if(_serializedData == null) throw new InvalidOperationException("Cannot call Undo before Redo");

            if(target.Scene.Components.HasComponent<Transform>(_entityId)
               && !target.Scene.Components.HasComponent(_type, _entityId)) {
                var loadOp = new LoadOperation(new MemoryStream(_serializedData));

                target.Scene.Deserialize(loadOp);

                loadOp.Close();
                loadOp.Dispose();
            } else {
                SceneEditor.ReportEditorGameConflict();
            }

            return true;
        }
    }
}