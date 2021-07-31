using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Util;

namespace FoldEngine {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class FoldGame : Game {
        public readonly GraphicsDeviceManager Graphics;
        private SpriteBatch _spriteBatch;

        private readonly IGameCore _core;

        private FixedSizeFloatBuffer FrameTimes = new FixedSizeFloatBuffer(60);
        
        private Point _lastKnownWindowSize = Point.Zero;

        public FoldGame() {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public FoldGame(IGameCore core) {
            this._core = core;

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 60);
            Graphics.SynchronizeWithVerticalRetrace = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            _core.RenderingUnit.Textures = new TextureManager(Graphics, GraphicsDevice, _spriteBatch, Content);
            _core.RenderingUnit.Fonts = new FontManager(_core.RenderingUnit.Textures);

            _core.AudioUnit._content = Content;

            _core.Initialize();
            
            (Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight) = _core.RenderingUnit.WindowSize;
            _lastKnownWindowSize = _core.RenderingUnit.WindowSize;
            
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (sender, args) => {
                Point newSize = Window.ClientBounds.Size;
                if(_lastKnownWindowSize != newSize) {
                    _lastKnownWindowSize = newSize;
                    _core.RenderingUnit.WindowSize = newSize;
                }
            };

            FoldEngine.Components.Component.PopulateIdentifiers();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _core.LoadContent();
            _core.RenderingUnit.LoadContent();
            // Create a new SpriteBatch, which can be used to draw textures.

            // foreach(IRenderingLayer layer in _core.RenderingUnit.RootGroup.Layers.Values) {
            //     layer.Surface = new RenderSurface(Graphics.GraphicsDevice, _core.RenderingUnit, layer.LayerSize.X,
            //         layer.LayerSize.Y);
            // }

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
               || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Exiting via input: ");
                if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
                    Console.WriteLine("Gamepad");
                } else {
                    Console.WriteLine("Keyboard");
                }

                Exit();
            }


            Time.Update(gameTime);
            Time.Frame++;

            _core.AudioUnit.Update();

            _core.InputUnit.Update();
            _core.Input();

            _core.Update();



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            Time.Update(gameTime);
            FrameTimes.Put(Time.DeltaTime);
            // Console.WriteLine("FPS: ");
            Time.FramesPerSecond = (1 / FrameTimes.Average());
            // TODO: Add your drawing code here

            GraphicsDevice.SetRenderTarget(null);

            
            _core.RenderingUnit.RootGroup.Begin();
            
            _core.Render();

            _core.RenderingUnit.RootGroup.End();

            
            //Draw each layer's buffer onto the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(new Color(0, 0, 0));
            
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            _core?.RenderingUnit.RootGroup.Present(_spriteBatch);
            
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
