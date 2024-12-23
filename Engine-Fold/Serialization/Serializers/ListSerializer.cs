﻿using System;
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
        int length = reader.ReadInt32();
        var list = new List<T>(length);
        for (int i = 0; i < length; i++) list.Add(reader.Read<T>());

        return list;
    }
}