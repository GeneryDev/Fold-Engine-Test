using FmodForFoxes;
using FoldEngine;
using FoldEngine.Interfaces;
using Sandbox;

var platformData = new GameRuntimeConfiguration()
{
    PlatformName = "WindowsDesktop",
    FmodNativeLibrary = new DesktopNativeFmodLibrary()
};

FoldGameEntry.StartGame(new WooferGameCore(platformData));