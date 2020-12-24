using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics {
    public class TextureAtlas {
        private GraphicsDevice _device;
        private SpriteBatch _batch;
        private RenderTarget2D TargetTexture;
        private List<KeyValuePair<string, Texture2D>> _containedTextures = new List<KeyValuePair<string, Texture2D>>();

        public Texture2D Texture => TargetTexture;

        public TextureAtlas(GraphicsDevice device) {
            _device = device;
            _batch = new SpriteBatch(device);
        }

        public void AddTexture(string identifier, Texture2D texture) {
            _containedTextures.Add(new KeyValuePair<string, Texture2D>(identifier, texture));
        }

        public void Pack() {
            int totalArea = _containedTextures.Select(pair => pair.Value.Width * pair.Value.Height)
                .Aggregate((accumulator, current) => accumulator + current);

            int initialDimensions = (int) Math.Pow(2, Math.Ceiling(Math.Log(Math.Ceiling(Math.Sqrt(totalArea))) / Math.Log(2)));
            
            TargetTexture = new RenderTarget2D(_device, initialDimensions, initialDimensions);
            _batch.Begin();
            
            _containedTextures.Sort((a, b) => b.Value.Width * b.Value.Height - a.Value.Width * a.Value.Height);
            
            List<Rectangle> availableRects = new List<Rectangle>();
            availableRects.Add(new Rectangle(0, 0, initialDimensions, initialDimensions));
            
            foreach(KeyValuePair<string, Texture2D> pair in _containedTextures) {
                Texture2D texture = pair.Value;
                int textureArea = texture.Width * texture.Height;

                Rectangle firstFit = availableRects.OrderBy(b => b.Width * b.Height)
                    .FirstOrDefault(rect => rect.Width >= texture.Width && rect.Height >= texture.Height);
                if(firstFit == Rectangle.Empty) {
                    throw new Exception("Cannot fit texture " + pair.Key + " into atlas");
                } else {
                    availableRects.Remove(firstFit);
                    _batch.Draw(texture, new Vector2(firstFit.X, firstFit.Y), Color.White);

                    bool choice = false;
                    if(choice) {
                        Rectangle a = new Rectangle(firstFit.X + texture.Width, firstFit.Y, firstFit.Width - texture.Width, firstFit.Height);
                        Rectangle b = new Rectangle(firstFit.X, firstFit.Y + texture.Height, texture.Width, firstFit.Height - texture.Height);
                        
                        availableRects.Add(a);
                        availableRects.Add(b);
                    } else {
                        Rectangle a = new Rectangle(firstFit.X + texture.Width, firstFit.Y, firstFit.Width - texture.Width, texture.Height);
                        Rectangle b = new Rectangle(firstFit.X, firstFit.Y + texture.Height, firstFit.Width, firstFit.Height - texture.Height);
                        
                        availableRects.Add(a);
                        availableRects.Add(b);
                    }
                }
                
            }
            
            _device.SetRenderTarget(TargetTexture);
            _device.Clear(Color.TransparentBlack);
            _batch.End();
            _device.SetRenderTarget(null);
        }
    }
}