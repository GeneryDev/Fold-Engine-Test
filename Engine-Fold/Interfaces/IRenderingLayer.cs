using System;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Interfaces;

public interface IRenderingLayer
{
    IRenderingUnit RenderingUnit { get; }
    RenderGroup Group { get; set; }

    string Name { get; }
    Point LayerSize { get; }
    Vector2 LogicalSize { get; }
    Rectangle Destination { get; set; }
    SamplerState Sampling { get; }

    RenderSurface Surface { get; set; }
    Color? Color { get; set; }
    bool IsRoot { get; set; }

    Vector2 CameraToLayer(Vector2 point);
    Vector2 LayerToCamera(Vector2 point);
    Vector2 LayerToLayer(Vector2 point, IRenderingLayer other);

    Vector2 WindowToLayer(Vector2 point);
    Vector2 LayerToWindow(Vector2 point);

    void WindowSizeChanged(Point oldSize, Point newSize);
    void Begin();
    void End();
}

public class RenderingLayer : IRenderingLayer
{
    private Point _layerSize;

    public RenderingLayer(IRenderingUnit renderer)
    {
        RenderingUnit = renderer;
    }

    public bool FitToWindow { get; set; } = false;
    private bool _isRoot;

    public bool IsRoot
    {
        get => _isRoot;
        set
        {
            bool changed = _isRoot != value;
            _isRoot = value;
            if (changed)
            {
                WindowSizeChanged(RenderingUnit.WindowSize, RenderingUnit.WindowSize);
            }
        }
    }

    public IRenderingUnit RenderingUnit { get; }
    public RenderGroup Group { get; set; }

    public string Name { get; set; }

    public Point LayerSize
    {
        get => _layerSize;
        set
        {
            if (Surface != null)
                Surface.Resize(value.X, value.Y);
            else
                Surface = new RenderSurface(this, value.X, value.Y);
            _layerSize = value;
        }
    }

    public Vector2 LogicalSize { get; set; }
    public Rectangle Destination { get; set; }
    public SamplerState Sampling { get; set; } = SamplerState.PointClamp;

    public RenderSurface Surface { get; set; }
    public Color? Color { get; set; }


    public Vector2 CameraToLayer(Vector2 point)
    {
        // (int x, int y, int width, int height) = Destination;
        //
        // double scaleX = (double) width / LayerSize.X;
        // double scaleY = (double) height / LayerSize.Y;
        //
        // point.X = (int) Math.Round(x + point.X * scaleX) + LayerSize.X / 2;
        // point.Y = (int) Math.Round(y - point.Y * scaleY) + LayerSize.Y / 2;

        point.Y *= -1;

        point *= LayerSize.ToVector2() / LogicalSize;

        point += LayerSize.ToVector2() / 2f;

        return point;
    }


    public Vector2 LayerToLayer(Vector2 point, IRenderingLayer to)
    {
        Vector2 relativeScale = to.LayerSize.ToVector2() / LayerSize.ToVector2();

        return (point - new Vector2(LayerSize.X / 2f, LayerSize.Y / 2f)) * relativeScale
               + new Vector2(to.LayerSize.X / 2f, to.LayerSize.Y / 2f);
    }

    public Vector2 LayerToCamera(Vector2 point)
    {
        return new Vector2(1, -1) * (point - LayerSize.ToVector2() / 2f) * (LogicalSize / LayerSize.ToVector2());
    }

    public Vector2 WindowToLayer(Vector2 point)
    {
        Rectangle groupBounds = Group.Bounds;

        point -= groupBounds.Location.ToVector2();
        point /= groupBounds.Size.ToVector2();
        point *= Group.Size.ToVector2();
        point -= Destination.Location.ToVector2();
        point /= Destination.Size.ToVector2();
        point *= LayerSize.ToVector2();
        return point;
    }

    public Vector2 LayerToWindow(Vector2 point)
    {
        Rectangle groupBounds = Group.Bounds;

        point /= LayerSize.ToVector2();
        point *= Destination.Size.ToVector2();
        point += Destination.Location.ToVector2();
        point /= Group.Size.ToVector2();
        point *= groupBounds.Size.ToVector2();
        point += groupBounds.Location.ToVector2();
        return point;
    }

    public void WindowSizeChanged(Point oldSize, Point newSize)
    {
        if (FitToWindow && IsRoot)
        {
            LayerSize = newSize;
            Destination = new Rectangle(Point.Zero, LayerSize);
            LogicalSize = newSize.ToVector2();
        }
    }

    public void Begin()
    {
        Surface?.Begin();
    }

    public void End()
    {
        Surface?.End();
    }
}

public class DependencyRenderingLayer : IRenderingLayer
{
    public int DependencyIndex;

    public DependencyRenderingLayer(int index)
    {
        DependencyIndex = index;
    }

    public IRenderingUnit RenderingUnit { get; }
    public RenderGroup Group { get; set; }
    public string Name { get; }
    public Point LayerSize { get; }
    public Vector2 LogicalSize { get; }
    public Rectangle Destination { get; set; }
    public SamplerState Sampling { get; }
    public RenderSurface Surface { get; set; }
    public Color? Color { get; set; }
    public bool IsRoot { get; set; }

    public Vector2 CameraToLayer(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public Vector2 LayerToCamera(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public Vector2 LayerToLayer(Vector2 point, IRenderingLayer other)
    {
        throw new NotImplementedException();
    }

    public Vector2 WindowToLayer(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public Vector2 LayerToWindow(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public void WindowSizeChanged(Point oldSize, Point newSize)
    {
    }

    public void Begin()
    {
    }

    public void End()
    {
    }
}