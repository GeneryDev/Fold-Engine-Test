using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntryProject.Util.JsonSerialization;
using FoldEngine;
using FoldEngine.Commands;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;

using Sandbox;

namespace Woofer {
    public class WooferGameCore : IGameCore {
        public FoldGame FoldGame { get; set; }

        public IRenderingUnit RenderingUnit { get; private set; }

        public Scene ActiveScene { get; private set; }

        public InputUnit InputUnit { get; private set; }

        public AudioUnit AudioUnit { get; private set; }

        public CommandQueue CommandQueue { get; private set; }

        public ResourceCollections Resources { get; private set; }

        public float TimeScale => 1;

        public WooferGameCore() {
            Console.WriteLine("Constructing Core");
            FoldGame = new FoldGame(this);
            
            RenderingUnit = new WooferRenderingUnit(this);
            ActiveScene = new DemoScene(this);
            InputUnit = new InputUnit();
            AudioUnit = new AudioUnit();
            CommandQueue = new CommandQueue(this);
            Resources = new ResourceCollections();

            InputUnit.Setup("Content/Config/input.json");

        }

        public void Initialize() {
            Console.WriteLine("Initializing Core");
            RenderingUnit.Initialize();
        }

        public void LoadContent() {
            Console.WriteLine("Loading Core Content");
            AudioUnit.Load("Audio/failure");
            AudioUnit.Load("Audio/music");
        }

        public void Input() {
            ActiveScene.Input();
        }

        public void Update() {
            ActiveScene.Update();
        }

        public void Render() {
            ActiveScene?.Render(RenderingUnit);
        }

        public static void Main() {
            Console.WriteLine("Started");
            FoldGameEntry.StartGame(new WooferGameCore());
        }
    }
}