using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

using Sandbox;

namespace Woofer
{
    public class WooferGameController : IGameController
    {
        public IRenderingUnit RenderingUnit { get; private set; }

        public float TimeScale => 1;

        public Scene ActiveScene { get; private set; }

        public WooferGameController() {
            ActiveScene = new DemoScene(this);
            RenderingUnit = new WooferRenderingUnit(this);
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
            FoldGameEntry.StartGame(new WooferGameController());
        }
    }
}
