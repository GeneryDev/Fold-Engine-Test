using System;
using FoldEngine.Physics;

namespace EntryProject.Editor.Inspector {
    public class ShowOnlyIf : Attribute {
        public string FieldName;
        public object Value;
        
        public ShowOnlyIf(string fieldName, object value) {
            this.FieldName = fieldName;
            this.Value = value;
        }
        
        public class Not : Attribute {
            public string FieldName;
            public object Value;
        
            public Not(string fieldName, object value) {
                this.FieldName = fieldName;
                this.Value = value;
            }
        }
    }
}