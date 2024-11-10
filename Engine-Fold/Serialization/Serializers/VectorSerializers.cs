using System;
using Microsoft.Xna.Framework;

namespace FoldEngine.Serialization.Serializers;

public class Vector2Serializer : Serializer<Vector2>
{
    public override Type WorkingType => typeof(Vector2);

    public override void Serialize(Vector2 vec, SaveOperation writer)
    {
        writer.Write(vec.X).Write(vec.Y);
    }

    public override Vector2 Deserialize(LoadOperation reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}

public class Vector3Serializer : Serializer<Vector3>
{
    public override Type WorkingType => typeof(Vector3);

    public override void Serialize(Vector3 vec, SaveOperation writer)
    {
        writer.Write(vec.X).Write(vec.Y).Write(vec.Z);
    }

    public override Vector3 Deserialize(LoadOperation reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}

public class Vector4Serializer : Serializer<Vector4>
{
    public override Type WorkingType => typeof(Vector4);

    public override void Serialize(Vector4 vec, SaveOperation writer)
    {
        writer.Write(vec.X).Write(vec.Y).Write(vec.Z).Write(vec.W);
    }

    public override Vector4 Deserialize(LoadOperation reader)
    {
        return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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
            ;
    }

    public override Matrix Deserialize(LoadOperation reader)
    {
        return new Matrix(
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
            reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
        );
    }
}