using System.Reflection;
using FmodForFoxes;

namespace FoldEngine.Interfaces;

public struct GameRuntimeConfiguration
{
    public string PlatformName;
    public INativeFmodLibrary FmodNativeLibrary;
    public Assembly[] AdditionalAssembliesForReflection;
}