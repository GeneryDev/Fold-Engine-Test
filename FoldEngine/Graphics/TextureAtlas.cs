using System;
using System.Collections.Generic;
using System.Linq;
using FoldEngine.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class TextureAtlas {
        private readonly ResourceCollections _resources;
        private readonly SpriteBatch _batch;

        private readonly List<KeyValuePair<string, ITexture>> _containedTextures =
            new List<KeyValuePair<string, ITexture>>();

        private readonly Dictionary<string, Rectangle> _textureBounds = new Dictionary<string, Rectangle>();
        private RenderTarget2D TargetTexture;

        public TextureAtlas(string name, ResourceCollections resources) {
            Name = name;
            _resources = resources;
            _batch = new SpriteBatch(FoldGame.Game.GraphicsDevice);
        }

        public string Name { get; }

        public Texture2D Texture => TargetTexture;

        public void AddTexture(string identifier, ITexture texture) {
            _containedTextures.Add(new KeyValuePair<string, ITexture>(identifier, texture));
        }

        public string Pack() {
            // _containedTextures.Sort((a, b) => b.Value.Width * b.Value.Height - a.Value.Width * a.Value.Height); //Largest to smallest (area)
            // _containedTextures.Sort((a, b) => a.Value.Width * a.Value.Height - b.Value.Width * b.Value.Height); //Smallest to largest (area)
            _containedTextures.Sort((a, b) =>
                Math.Max(b.Value.Width, b.Value.Height)
                - Math.Max(a.Value.Width, a.Value.Height)); //Largest to smallest (max axis) //seems best
            // _containedTextures.Sort((a, b) => Math.Max(a.Value.Width, a.Value.Height) - Math.Max(b.Value.Width, b.Value.Height)); //Smallest to largest (max axis)

            var availableRects = new List<Rectangle>();
            availableRects.Add(new Rectangle(0, 0, (int) Math.Sqrt(int.MaxValue), (int) Math.Sqrt(int.MaxValue)));
            var usedRect = Rectangle.Empty;

            _batch.Begin();
            foreach(KeyValuePair<string, ITexture> pair in _containedTextures) {
                ITexture texture = pair.Value;

                Rectangle firstFit = availableRects.OrderBy(b => b.Location.ToVector2().LengthSquared())
                    .FirstOrDefault(rect => rect.Width >= texture.Width && rect.Height >= texture.Height);
                if(firstFit == Rectangle.Empty) throw new Exception("Cannot fit texture " + pair.Key + " into atlas");

                var thisTextureBounds = new Rectangle(firstFit.X, firstFit.Y, texture.Width, texture.Height);

                _textureBounds[pair.Key] = thisTextureBounds;

                availableRects.Remove(firstFit);
                usedRect = Rectangle.Union(usedRect, thisTextureBounds);
                texture.DrawOnto(_batch, new Vector2(firstFit.X, firstFit.Y));

                bool choice = true;
                if(choice) {
                    var a = new Rectangle(firstFit.X + texture.Width, firstFit.Y, firstFit.Width - texture.Width,
                        firstFit.Height);
                    var b = new Rectangle(firstFit.X, firstFit.Y + texture.Height, texture.Width,
                        firstFit.Height - texture.Height);

                    availableRects.Add(a);
                    availableRects.Add(b);
                } else {
                    var a = new Rectangle(firstFit.X + texture.Width, firstFit.Y, firstFit.Width - texture.Width,
                        texture.Height);
                    var b = new Rectangle(firstFit.X, firstFit.Y + texture.Height, firstFit.Width,
                        firstFit.Height - texture.Height);

                    availableRects.Add(a);
                    availableRects.Add(b);
                }
            }

            int finalWidth = (int) Math.Pow(2, Math.Ceiling(Math.Log(usedRect.Width) / Math.Log(2)));
            int finalHeight = (int) Math.Pow(2, Math.Ceiling(Math.Log(usedRect.Height) / Math.Log(2)));

            TargetTexture = new RenderTarget2D(FoldGame.Game.GraphicsDevice, finalWidth, finalHeight);

            string createdIdentifier = "__atlas." + Name;

            Texture atlas = _resources.Create<Texture>(createdIdentifier).Direct(TargetTexture);
            foreach(KeyValuePair<string, Rectangle> pair in _textureBounds)
                _resources.Create<Texture>(pair.Key).Atlased(atlas, pair.Value);

            FoldGame.Game.GraphicsDevice.SetRenderTarget(TargetTexture);
            FoldGame.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            _batch.End();
            FoldGame.Game.GraphicsDevice.SetRenderTarget(null);

            return createdIdentifier;
        }
    }
}