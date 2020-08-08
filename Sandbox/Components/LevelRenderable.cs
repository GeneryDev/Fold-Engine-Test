using FoldEngine.Components;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Components
{
    [Component("sandbox:level_renderable")]
    public class LevelRenderable : Component
    {
        public float ZOrder { get; set; }

    }
}
