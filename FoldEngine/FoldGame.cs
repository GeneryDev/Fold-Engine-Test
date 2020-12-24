using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Util;

namespace FoldEngine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class FoldGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly IGameController _controller;

        public FoldGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public FoldGame(IGameController controller)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this._controller = controller;

            (_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight) = _controller.RenderingUnit.ScreenSize;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _controller.Initialize();

            FoldEngine.Components.Component.PopulateIdentifiers();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _controller.RenderingUnit.Textures = new TextureManager(_graphics, GraphicsDevice, _spriteBatch, Content);

            foreach (IRenderingLayer layer in _controller.RenderingUnit.Layers.Values)
            {
                layer.Surface = new RenderSurface(_graphics.GraphicsDevice, layer.LayerSize.X, layer.LayerSize.Y);
            }

            // TODO: use this.Content to load your game content here

            _controller.RenderingUnit.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            
            Time.Update(gameTime);

            _controller.Input();
            _controller.Update();



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Time.Update(gameTime);
            // TODO: Add your drawing code here

            GraphicsDevice.SetRenderTarget(null);

            //Begin sprite batches
            
            foreach (IRenderingLayer layer in _controller.RenderingUnit.Layers.Values)
            {
                layer.Surface.Begin();
            }

            //Draw the scene, across multiple sprite batches
            
            _controller.RenderingUnit.Render();

            //Resolve each sprite batch into each layer's buffer
            foreach (IRenderingLayer layer in _controller.RenderingUnit.Layers.Values)
            {
                layer.Surface.End();
            }

            //Draw each layer's buffer onto the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            if (_controller != null)
                foreach (IRenderingLayer layer in _controller.RenderingUnit.Layers.Values)
                {
                    _spriteBatch.Draw(layer.Surface.Target, layer.Destination, Color.White);
                }
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
