using System;

namespace EntryProject.Editor.Inspector {
    public class ShowOnlyIf : Attribute {
        public string FieldName;
        public object Value;

        public ShowOnlyIf(string fieldName, object value) {
            FieldName = fieldName;
            Value = value;
        }

        public class Not : Attribute {
            public string FieldName;
            public object Value;

            public Not(string fieldName, object value) {
                FieldName = fieldName;
                Value = value;
            }
        }
    }
}