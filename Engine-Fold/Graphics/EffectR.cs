using System;
using FoldEngine.IO;
using FoldEngine.Resources;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics;

[Resource("effect", extensions: "fx")]
public class EffectR : Resource
{
    public Effect Effect { get; private set; }
    public int Order;

    public override bool CanSerialize => false;

    public override void DeserializeResource(string path)
    {
        try
        {
            Effect = EffectManager.Compile(Data.In.Path(path), out string source);
            if (source.StartsWith("//order:"))
            {
                int.TryParse(source.Substring("//order:".Length, source.IndexOf('\n') - "//order:".Length).Trim(),
                    out Order);
            }

            Console.WriteLine($"Order for effect {Identifier}: {Order}");
        }
        catch (Exception x)
        {
            Console.WriteLine($"Error compiling shader {Identifier}: {x.Message}");
        }
    }
}