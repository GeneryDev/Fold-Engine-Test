using FoldEngine.Components;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Components
{
    [Component("sandbox:living")]
    public struct Living
    {
        public int Health;
        public int MaxHealth;

        public override string ToString()
        {
            return $"sandbox:living|{Health}/{MaxHealth}";
        }
    }
}
