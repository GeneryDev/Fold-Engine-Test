using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntryProject.Util.JsonSerialization;
using FoldEngine;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

using Sandbox;

namespace Woofer
{
    public class WooferGameCore : IGameCore
    {
        public IRenderingUnit RenderingUnit { get; private set; }

        public Scene ActiveScene { get; private set; }
        
        public IInputUnit InputUnit { get; private set; }

        public float TimeScale => 1;

        public WooferGameCore() {
            RenderingUnit = new WooferRenderingUnit(this);
            ActiveScene = new DemoScene(this);
            InputUnit = new IInputUnit();

            InputUnit.Setup("Content/Config/input.json");
            
        }

        public void Initialize()
        {
            
        }
        public void Input()
        {
            ActiveScene.Input();
        }

        public void Update()
        {
            ActiveScene.Update();
        }

        public void Render() {
            ActiveScene?.Render(RenderingUnit);
        }

        public static void Main()
        {
            Console.WriteLine("Started");
            FoldGameEntry.StartGame(new WooferGameCore());
        }
    }
}
