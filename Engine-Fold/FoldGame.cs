using System;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine {
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class FoldGame : Game {
        public static FoldGame Game;

        public readonly IGameCore Core;

        public readonly GameRuntimeConfiguration RuntimeConfig;
        
        public readonly GraphicsDeviceManager Graphics;

        private Point _lastKnownWindowSize = Point.Zero;
        private SpriteBatch _spriteBatch;

        private readonly FixedSizeFloatBuffer FrameTimes = new FixedSizeFloatBuffer(60);

        public FoldGame(IGameCore core, GameRuntimeConfiguration runtimeConfig) {
            Core = core;
            Game = this;
            RuntimeConfig = runtimeConfig;

            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 120);
            Graphics.SynchronizeWithVerticalRetrace = false;
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            Core.Initialize();

            (Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight) = Core.RenderingUnit.WindowSize;
            _lastKnownWindowSize = Core.RenderingUnit.WindowSize;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (sender, args) => {
                Point newSize = Window.ClientBounds.Size;
                if(_lastKnownWindowSize != newSize) {
                    _lastKnownWindowSize = newSize;
                    Core.RenderingUnit.WindowSize = newSize;
                }
            };

            Component.PopulateIdentifiers();

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Core.LoadContent();
            Core.RenderingUnit.LoadContent();
            // Create a new SpriteBatch, which can be used to draw textures.

            // foreach(IRenderingLayer layer in _core.RenderingUnit.RootGroup.Layers.Values) {
            //     layer.Surface = new RenderSurface(Graphics.GraphicsDevice, _core.RenderingUnit, layer.LayerSize.X,
            //         layer.LayerSize.Y);
            // }

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
               || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Exiting via input: ");
                if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    Console.WriteLine("Gamepad");
                else
                    Console.WriteLine("Keyboard");

                Exit();
            }


            Time.Update(gameTime);
            Time.Frame++;

            Core.AudioUnit.Update();

            Core.InputUnit.Update();
            Core.Input();

            Core.Resources.Update();

            Core.Update();
            if(Core.ActiveScene == null) {
                Core.CommandQueue.ExecuteAll();
            }


            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            Time.Update(gameTime);
            FrameTimes.Put(Time.DeltaTime);
            // Console.WriteLine("FPS: ");
            Time.FramesPerSecond = 1 / FrameTimes.Average();
            // TODO: Add your drawing code here

            GraphicsDevice.SetRenderTarget(null);

            Core.RenderingUnit.RootGroup.IsRoot = true;
            Core.RenderingUnit.RootGroup.Begin();

            Core.Render();

            Core.RenderingUnit.RootGroup.End();


            //Draw each layer's buffer onto the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Core?.RenderingUnit.RootGroup.Present(_spriteBatch);

            _spriteBatch.End();

            //spriteBatch.Draw(testTex, new Rectangle((int)(gameTime.TotalGameTime.TotalMilliseconds / 10), 16, 16, 16), Color.White);
            //spriteBatch.End();


            //Draw to screen
            //GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            //spriteBatch.Draw(mainTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            //spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}