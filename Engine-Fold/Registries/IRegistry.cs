using System;

namespace FoldEngine.Registries;

public interface IRegistry
{
    public void AcceptType(Type type);
}