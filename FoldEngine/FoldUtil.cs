using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine
{
    public class FoldUtil
    {
        public static bool AssertsEnabled = true;
        public static void Assert(bool value, string message)
        {
            if(AssertsEnabled && !value)
            {
                throw new Exception("Assertion Failed: " + message);
            }
        }

        public static void Breakpoint() {
            bool a = true;
        }
    }
}
