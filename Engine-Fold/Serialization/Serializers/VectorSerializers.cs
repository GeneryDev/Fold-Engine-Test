using System;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Serialization.Serializers;

public class PointSerializer : Serializer<Point>
{
    public override Type WorkingType => typeof(Point);

    public override void Serialize(Point vec, SaveOperation writer)
    {
        writer.StartStruct().Write(vec.X).Write(vec.Y).EndStruct();
    }

    public override Point Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new Point(reader.ReadInt32(), reader.ReadInt32());
        reader.EndStruct();
        return result;
    }
}

public class Vector2Serializer : Serializer<Vector2>
{
    public override Type WorkingType => typeof(Vector2);

    public override void Serialize(Vector2 vec, SaveOperation writer)
    {
        writer.StartStruct().Write(vec.X).Write(vec.Y).EndStruct();
    }

    public override Vector2 Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        reader.EndStruct();
        return result;
    }
}

public class Vector3Serializer : Serializer<Vector3>
{
    public override Type WorkingType => typeof(Vector3);

    public override void Serialize(Vector3 vec, SaveOperation writer)
    {
        writer.StartStruct().Write(vec.X).Write(vec.Y).Write(vec.Z).EndStruct();
    }

    public override Vector3 Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        reader.EndStruct();
        return result;
    }
}

public class Vector4Serializer : Serializer<Vector4>
{
    public override Type WorkingType => typeof(Vector4);

    public override void Serialize(Vector4 vec, SaveOperation writer)
    {
        writer.StartStruct().Write(vec.X).Write(vec.Y).Write(vec.Z).Write(vec.W).EndStruct();
    }

    public override Vector4 Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        reader.EndStruct();
        return result;
    }
}

public class ColorSerializer : Serializer<Color>
{
    public override Type WorkingType => typeof(Color);

    public override void Serialize(Color vec, SaveOperation writer)
    {
        writer.Write(vec.PackedValue);
    }

    public override Color Deserialize(LoadOperation reader)
    {
        return new Color(reader.ReadUInt32());
    }
}

public class MatrixSerializer : Serializer<Matrix>
{
    public override Type WorkingType => typeof(Matrix);

    public override void Serialize(Matrix mat, SaveOperation writer)
    {
        writer
            .StartStruct()
            .Write(mat.M11)
            .Write(mat.M12)
            .Write(mat.M13)
            .Write(mat.M14)
            .Write(mat.M21)
            .Write(mat.M22)
            .Write(mat.M23)
            .Write(mat.M24)
            .Write(mat.M31)
            .Write(mat.M32)
            .Write(mat.M33)
            .Write(mat.M34)
            .Write(mat.M41)
            .Write(mat.M42)
            .Write(mat.M43)
            .Write(mat.M44)
            .EndStruct()
            ;
    }

    public override Matrix Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new Matrix(
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
        );
        reader.EndStruct();
        return result;
    }
}

public class LRTBSerializer : Serializer<LRTB>
{
    public override Type WorkingType => typeof(LRTB);

    public override void Serialize(LRTB vec, SaveOperation writer)
    {
        writer.StartStruct().Write(vec.Left).Write(vec.Right).Write(vec.Top).Write(vec.Bottom).EndStruct();
    }

    public override LRTB Deserialize(LoadOperation reader)
    {
        reader.StartStruct();
        var result = new LRTB(Left: reader.ReadSingle(), Right: reader.ReadSingle(), Top: reader.ReadSingle(), Bottom: reader.ReadSingle());
        reader.EndStruct();
        return result;
    }
}