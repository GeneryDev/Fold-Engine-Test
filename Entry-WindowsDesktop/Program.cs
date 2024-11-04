using FmodForFoxes;
using FoldEngine;
using FoldEngine.Interfaces;
using Woofer;

var platformData = new GameRuntimeConfiguration()
{
    PlatformName = "WindowsDesktop",
    FmodNativeLibrary = new DesktopNativeFmodLibrary()
};

FoldGameEntry.StartGame(new WooferGameCore(platformData));