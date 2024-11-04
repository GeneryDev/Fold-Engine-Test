using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FoldEngine.Interfaces;
using FoldEngine.Registries;

namespace FoldEngine.Resources;

public class ResourceRegistry : IRegistry
{
    private readonly Dictionary<Type, ConstructorInfo> Constructors = new();

    private Dictionary<Type, ResourceAttribute> _attributes = new();
    private Dictionary<string, ResourceAttribute> _attributesById = new();
    
    public IResourceCollection CreateCollectionForType(Type resourceType, IGameCore core)
    {
        ConstructorInfo constructor;
        if(!Constructors.TryGetValue(resourceType, out constructor))
            Constructors[resourceType] = constructor =
                typeof(ResourceCollection<>).MakeGenericType(resourceType).GetConstructor(Type.EmptyTypes);

        var collection = (IResourceCollection) constructor.Invoke(Array.Empty<object>());
        collection.Core = core;
        return collection;
    }

    public void AcceptType(Type type)
    {
        if(type.IsSubclassOf(typeof(Resource))) {
            var attribute = type.GetCustomAttribute<ResourceAttribute>(false);
            if(attribute != null) {
                attribute.ResourceType = type;
                _attributes[type] = attribute;
                _attributesById[attribute.Identifier] = attribute;
            }
        }
    }

    public ResourceAttribute AttributeOf(Type type) {
        if(_attributes.ContainsKey(type)) {
            return _attributes[type];
        } else {
            foreach(Type key in _attributes.Keys) {
                if(type.IsSubclassOf(key)) return _attributes[key];
            }

            return null;
        }
    }

    public ResourceAttribute AttributeOf(string identifier) {
        return _attributesById.ContainsKey(identifier) ? _attributesById[identifier] : null;
    }

    public ResourceAttribute AttributeOf<T>() where T : Resource {
        return AttributeOf(typeof(T));
    }

    public Dictionary<Type, ResourceAttribute>.KeyCollection GetAllTypes() {
        return _attributes.Keys;
    }

    public string GetResourcePath(Resource resource)
    {
        ResourceAttribute resourceAttribute = AttributeOf(resource.GetType());
        return resourceAttribute.CreateResourcePath(resource.Identifier);
    }
}