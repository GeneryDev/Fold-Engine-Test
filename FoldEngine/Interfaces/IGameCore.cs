using System;
using FoldEngine.Commands;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace FoldEngine.Interfaces {
    /// <summary>
    /// Declares a master interface consisting of the data needed for a game to function
    /// </summary>
    public interface IGameCore {
        FoldGame FoldGame { get; }
        
        /// <summary>
        /// The scene that is currently being played
        /// </summary>
        Scene ActiveScene { get; }

        /// <summary>
        /// This Core's rendering unit
        /// </summary>
        IRenderingUnit RenderingUnit { get; }

        /// <summary>
        /// This Core's input unit
        /// </summary>
        InputUnit InputUnit { get; }

        /// <summary>
        /// This Core's audio unit
        /// </summary>
        AudioUnit AudioUnit { get; }

        /// <summary>
        /// This Core's audio unit
        /// </summary>
        CommandQueue CommandQueue { get; }

        /// <summary>
        /// This Core's global resources
        /// </summary>
        ResourceCollections Resources { get; }

        /// <summary>
        /// This Core's global resource index, listing all resources available in disk.
        /// </summary>
        ResourceIndex ResourceIndex { get; }


        /// <summary>
        /// Runs once when the game starts
        /// </summary>
        void Initialize();

        /// <summary>
        /// Runs a fixed number of times per second
        /// </summary>
        void Update();

        /// <summary>
        /// Runs a fixed number of times when input information should be processed
        /// </summary>
        void Input();

        /// <summary>
        /// Called when the game should draw itself
        /// </summary>
        void Render();

        /// <summary>
        /// Runs after Initialize to load game content
        /// </summary>
        void LoadContent();
    }
}