using System;

namespace FoldEngine.Editor.Inspector {
    public class NameAttribute : Attribute {
        public readonly string Name;

        public NameAttribute(string name) {
            Name = name;
        }
    }
}