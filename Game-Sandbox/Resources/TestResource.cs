﻿using FoldEngine.Resources;
using FoldEngine.Serialization;
using Microsoft.Xna.Framework;

namespace Sandbox.Resources;

[Resource("test")]
public class TestResource : Resource
{
    [FormerlySerializedAs("color")] public Color Color;
}