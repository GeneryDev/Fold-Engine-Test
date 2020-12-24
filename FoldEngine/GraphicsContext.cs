using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine
{
    public class GraphicsContext
    {
        private readonly GraphicsDeviceManager _manager;
        private readonly GraphicsDevice _device;
        private readonly SpriteBatch _spriteBatch;

        private RenderTarget2D _lastRenderTarget = null;

        private Texture2D _pixel = null;
        private Color _currentPixelColor = Color.Transparent;

        //Constructors
        public GraphicsContext(GraphicsDeviceManager manager, GraphicsDevice device, SpriteBatch spriteBatch)
        {
            this._manager = manager;
            this._device = device;
            this._spriteBatch = spriteBatch;
        }

        private void ChangeRenderTarget(RenderTarget2D surface)
        {
            if (_lastRenderTarget != surface)
            {
                _device.SetRenderTarget(surface);
                if (surface != null) _device.Clear(new Color(255, 255, 255, 0));
                _lastRenderTarget = surface;
            }
        }

        private void ChangePixelColor(FoldEngine.Util.Deprecated.Color color)
        {
            Color toXna = color;
            if (_currentPixelColor != toXna)
            {
                _pixel.SetData(new[] { toXna });
                _currentPixelColor = toXna;
            }
        }

        //Clear surface
        public void Clear(RenderTarget2D surface, FoldEngine.Util.Deprecated.Color color)
        {
            ChangeRenderTarget(surface);
            _device.Clear(new Color(color.R, color.G, color.B, color.A));
        }
        public void Clear(FoldEngine.Util.Deprecated.Color color) => Clear(null, color);

        //Create a target
        public RenderTarget2D CreateTarget(int width, int height)
        {
            RenderTarget2D target = new RenderTarget2D(_device, width, height);
            ChangeRenderTarget(target);
            return target;
        }

        //Create a source from target
        public Texture2D TargetToSource(RenderTarget2D target) => target;

        //Draw texture
        public void Draw(Texture2D subject, RenderTarget2D target, FoldEngine.Util.Deprecated.Rectangle destination, FoldEngine.Util.Deprecated.Rectangle? source = null, DrawInfo info = default(DrawInfo))
        {
            ChangeRenderTarget(target);
            _spriteBatch.Begin(SpriteSortMode.Immediate, info.Mode == DrawMode.Additive ? BlendState.Additive : info.Mode == DrawMode.Overlay ? BlendState.AlphaBlend : BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            _spriteBatch.Draw(subject, destination, source.HasValue ? source.Value : (Rectangle?)null, info.Color.HasValue ? info.Color.Value : Color.White);
            _spriteBatch.End();
        }

        public void FillRect(RenderTarget2D target, FoldEngine.Util.Deprecated.Rectangle rectangle, FoldEngine.Util.Deprecated.Color color) => FillRect(target, rectangle, new DrawInfo() { Color = color, Mode = DrawMode.Normal });

        public void FillRect(RenderTarget2D target, FoldEngine.Util.Deprecated.Rectangle rectangle, DrawInfo info)
        {
            ChangeRenderTarget(target);
            _spriteBatch.Begin(SpriteSortMode.Immediate, blendState: ModeToBlend(info.Mode));
            if (_pixel == null)
            {
                _pixel = new Texture2D(_device, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            //ChangePixelColor(color);
            _spriteBatch.Draw(_pixel, rectangle, null, info.Color ?? Color.White);
            _spriteBatch.End();
        }

        public void DrawLine(RenderTarget2D target, FoldEngine.Util.Deprecated.VectorInt2 point1, FoldEngine.Util.Deprecated.VectorInt2 point2, FoldEngine.Util.Deprecated.Color color, int thickness)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            int length = (int)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));

            ChangeRenderTarget(target);
            _spriteBatch.Begin(SpriteSortMode.Immediate, blendState: null);
            if (_pixel == null)
            {
                _pixel = new Texture2D(_device, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            //ChangePixelColor(color);
            _spriteBatch.Draw(_pixel, new FoldEngine.Util.Deprecated.Rectangle(point1.X, point1.Y - thickness / 2, length, thickness), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
            _spriteBatch.End();
        }

        private BlendState ModeToBlend(DrawMode mode) => mode == DrawMode.Additive ? BlendState.Additive : mode == DrawMode.Overlay ? BlendState.AlphaBlend : BlendState.NonPremultiplied;

        //Retrieve size of texture or screen
        public FoldEngine.Util.Deprecated.VectorInt2 GetSize(Texture2D surface) => surface != null ? new FoldEngine.Util.Deprecated.VectorInt2(surface.Width, surface.Height) : GetScreenSize();// new FoldEngine.Util.Deprecated.Size(surface.Width, surface.Height);
        public FoldEngine.Util.Deprecated.VectorInt2 GetScreenSize() => new FoldEngine.Util.Deprecated.VectorInt2(_manager.PreferredBackBufferWidth, _manager.PreferredBackBufferHeight);

        //Scale texture
        public RenderTarget2D Scale(RenderTarget2D surface, double scaleX, double scaleY, bool antialias)
        {
            FoldEngine.Util.Deprecated.VectorInt2 newSize = new FoldEngine.Util.Deprecated.VectorInt2((int)(surface.Width * scaleX), (int)(surface.Height * scaleY));
            RenderTarget2D newTarget = CreateTarget(newSize.X, newSize.Y);
            Draw(surface, newTarget, new FoldEngine.Util.Deprecated.Rectangle(new FoldEngine.Util.Deprecated.VectorInt2(0, 0), newSize));
            return newTarget;
        }
        public RenderTarget2D Scale(RenderTarget2D surface, double scale, bool antialias) => Scale(surface, scale, scale, antialias);

        //Update screen
        public void Update(RenderTarget2D surface) => Draw(surface, null, new FoldEngine.Util.Deprecated.Rectangle(0, 0, _device.PresentationParameters.BackBufferWidth, _device.PresentationParameters.BackBufferHeight));
        public void Update(RenderTarget2D surface, FoldEngine.Util.Deprecated.Rectangle destination)
        {
            //System.Console.WriteLine(destination);
            Draw(surface, null, destination);
        }

        //Dispose surfaces
        public void DisposeSurface(RenderTarget2D surface) => surface.Dispose();
        public void DisposeSource(Texture2D source) => source.Dispose();

        public void Begin()
        {
            ChangeRenderTarget(null);
            _device.Clear(Color.Black);
        }
        public void Reset() => ChangeRenderTarget(null);
    }
    public enum DrawMode
    {
        Normal,
        Additive,
        Overlay
    }

    public struct DrawInfo
    {
        public DrawMode Mode;
        public Color? Color;
    }
}
