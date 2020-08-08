using FoldEngine.Components;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Components
{
    [Component("sandbox:living")]
    public class Living : ComponentAttachment
    {
        public int Health = 100;
        public int MaxHealth = 100;
    }
}
