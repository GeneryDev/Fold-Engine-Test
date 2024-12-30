using System;
using FoldEngine.IO;
using FoldEngine.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics;

public interface ITexture
{
    int Width { get; }
    int Height { get; }
    Texture2D Source { get; }
    void DrawOnto(SpriteBatch batch, Vector2 pos);
    void DrawOnto(SpriteBatch batch, Rectangle pos);
    Vector2 ToSourceUV(Vector2 uv);
    Rectangle CreateSubBounds(Rectangle bounds);
}

public class DirectTexture : ITexture
{
    public DirectTexture(Texture2D texture)
    {
        Texture = texture;
    }

    internal Texture2D Texture { get; }

    public Texture2D Source => Texture;
    public int Width => Texture.Width;
    public int Height => Texture.Height;

    public void DrawOnto(SpriteBatch batch, Vector2 pos)
    {
        batch.Draw(Texture, pos, Color.White);
    }

    public void DrawOnto(SpriteBatch batch, Rectangle pos)
    {
        batch.Draw(Texture, pos, Color.White);
    }

    public Rectangle CreateSubBounds(Rectangle bounds)
    {
        return bounds;
    }

    public Vector2 ToSourceUV(Vector2 uv)
    {
        return uv;
    }
}

public class AtlasedTexture : ITexture
{
    internal Rectangle Bounds;

    public AtlasedTexture(Texture2D texture, Rectangle bounds)
    {
        Texture = texture;
        Bounds = bounds;
    }

    internal Texture2D Texture { get; }

    public Texture2D Source => Texture;
    public int Width => Bounds.Width;
    public int Height => Bounds.Height;

    public void DrawOnto(SpriteBatch batch, Vector2 pos)
    {
        batch.Draw(Texture, pos, Bounds, Color.White);
    }

    public void DrawOnto(SpriteBatch batch, Rectangle pos)
    {
        batch.Draw(Texture, pos, Bounds, Color.White);
    }

    public Rectangle CreateSubBounds(Rectangle bounds)
    {
        return new Rectangle(Bounds.X + bounds.X, Bounds.Y + bounds.Y, bounds.Width, bounds.Height);
    }

    public Vector2 ToSourceUV(Vector2 uv)
    {
        return new Vector2(
            (Bounds.X + Width * uv.X) / Texture.Width,
            (Bounds.Y + Height * uv.Y) / Texture.Height
        );
    }
}

[Resource("texture", extensions: ["png"])]
public class Texture : Resource, ITexture
{
    public static Texture White;
    public static Texture Missing;

    private Rectangle _bounds;
    private Texture _parent;
    private Texture2D _texture;
    private bool _unloaded;

    public override bool CanSerialize => false;

    public Texture2D Source => _texture ?? _parent.Source;
    public int Width => _bounds.Width;
    public int Height => _bounds.Height;

    public Texture Parent => _parent;

    public void DrawOnto(SpriteBatch batch, Vector2 pos)
    {
        batch.Draw(_texture, pos, _bounds, Color.White);
    }

    public void DrawOnto(SpriteBatch batch, Rectangle pos)
    {
        batch.Draw(_texture, pos, _bounds, Color.White);
    }

    public Vector2 ToSourceUV(Vector2 uv)
    {
        return _texture != null
            ? uv
            : new Vector2(
                (_bounds.X + Width * uv.X) / Source.Width,
                (_bounds.Y + Height * uv.Y) / Source.Height
            );
    }

    public Rectangle CreateSubBounds(Rectangle bounds)
    {
        return _texture != null
            ? bounds
            : new Rectangle(_bounds.X + bounds.X, _bounds.Y + bounds.Y, bounds.Width, bounds.Height);
        ;
    }

    public Texture Direct(Texture2D texture)
    {
        _texture = texture;
        _parent = null;
        _bounds = new Rectangle(0, 0, texture.Width, texture.Height);
        return this;
    }

    public Texture Atlased(Texture parent, Rectangle bounds)
    {
        _texture = null;
        _parent = parent;
        _bounds = bounds;
        return this;
    }

    private Texture ConstantTexture(string identifier, Color[] colors)
    {
        Identifier = identifier;
        var tex = new RenderTarget2D(FoldGame.Game.GraphicsDevice, (int)Math.Sqrt(colors.Length),
            (int)Math.Sqrt(colors.Length));
        tex.SetData(colors);
        return Direct(tex);
    }

    public override void Access()
    {
        base.Access();
        _parent?.Access();
    }

    public override bool Unload()
    {
        if (_parent == null || _parent._unloaded)
        {
            _texture?.Dispose();
            _unloaded = true;
            return true;
        }

        return false;
    }

    public override void DeserializeResource(string path)
    {
        Direct(Texture2D.FromStream(FoldGame.Game.GraphicsDevice, Data.In.Stream(path)));
    }

    public static void CreateConstants()
    {
        White = new Texture().ConstantTexture("__WHITE", new[] { Color.White });
        Missing = new Texture().ConstantTexture("__MISSING",
            new[] { Color.Magenta, Color.Black, Color.Black, Color.Magenta });
    }
}