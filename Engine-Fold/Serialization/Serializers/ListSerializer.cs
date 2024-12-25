using System;
using System.Collections.Generic;

namespace FoldEngine.Serialization.Serializers;

public class ListSerializer<T> : Serializer<List<T>>
{
    public override Type WorkingType => typeof(T);

    public override void Serialize(List<T> t, SaveOperation writer)
    {
        writer.WriteArray((ref SaveOperation.Array arr) =>
        {
            foreach (var element in t)
            {
                arr.WriteMember(element);
            }
        });
    }

    public override List<T> Deserialize(LoadOperation reader)
    {
        var list = new List<T>();
        reader.ReadArray(a =>
            {
                list.EnsureCapacity(a.MemberCount);
                while (list.Count < a.MemberCount)
                {
                    list.Add(default);
                }
            }, m =>
            {
                list[m.Index] = reader.Read<T>();
            }
        );
        return list;
    }
}