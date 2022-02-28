using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntryProject.Util.JsonSerialization;
using FoldEngine;
using FoldEngine.Commands;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.IO;
using FoldEngine.Resources;
using FoldEngine.Scenes;

using Sandbox;

namespace Woofer {
    public class WooferGameCore : IGameCore {
        public FoldGame FoldGame { get; }

        public IRenderingUnit RenderingUnit { get; }

        public Scene ActiveScene { get; }

        public InputUnit InputUnit { get; }

        public AudioUnit AudioUnit { get; }

        public CommandQueue CommandQueue { get; }

        public ResourceCollections Resources { get; }

        public ResourceIndex ResourceIndex { get; }

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
            ResourceIndex = new ResourceIndex();
            ResourceIndex.Update();

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
#if DEBUG
            Console.WriteLine("Command Line Arguments: " + Environment.GetCommandLineArgs().Length);
            for(int i = 0; i < Environment.GetCommandLineArgs().Length; i++) {
                Console.WriteLine($"[{i}]: " + Environment.GetCommandLineArgs()[i]);
            }
#endif
            FoldGameEntry.StartGame(new WooferGameCore());
        }
    }
}