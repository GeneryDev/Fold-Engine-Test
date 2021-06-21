using System;
using System.Collections.Generic;
using System.Text;
using FoldEngine.Editor.Inspector;

namespace FoldEngine.Components
{
    [HideInInspector]
    [Component("name")]
    public struct EntityName {
        public string Name;
    }
}
