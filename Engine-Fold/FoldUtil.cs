using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine;

public class FoldUtil
{
    public static bool AssertsEnabled = true;

    public static void Assert(bool value, string message)
    {
        if (AssertsEnabled && !value) throw new Exception("Assertion Failed: " + message);
    }

    public static void Breakpoint()
    {
        bool a = true;
    }

    public static string EnumerableToString<T>(IEnumerable<T> enumerable)
    {
        var sb = new StringBuilder("{");
        foreach (T t in enumerable)
        {
            sb.Append(t);
            sb.Append(", ");
        }

        if (sb.Length > 1) sb.Length -= 2;
        sb.Append("}");
        return sb.ToString();
    }
}