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

        public float TimeScale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Scene ActiveScene { get; private set; } = new DemoScene();

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

        public WooferGameController()
        {
            RenderingUnit = new WooferRenderingUnit(this);
        }

        public static void Main()
        {
            Console.WriteLine("Started");
            FoldGameEntry.StartGame(new WooferGameController());
        }
    }
}
