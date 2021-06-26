using System;
using System.Reflection;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;

namespace FoldEngine.Editor.Transactions {
    public class SetComponentFieldTransaction : Transaction<EditorEnvironment> {

        public long EntityId;
        public Type ComponentType;
        public FieldInfo FieldInfo;
        
        public object OldValue;
        public object NewValue;
        
        
        public override bool Redo(EditorEnvironment target) {
            target.Scene.Components.Sets[ComponentType].SetFieldValue((int)EntityId, FieldInfo, NewValue);
            return OldValue != NewValue;
        }

        public override bool Undo(EditorEnvironment target) {
            target.Scene.Components.Sets[ComponentType].SetFieldValue((int)EntityId, FieldInfo, OldValue);
            return OldValue != NewValue;
        }
    }
}