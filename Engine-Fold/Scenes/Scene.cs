using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.IO;
using FoldEngine.Resources;
using FoldEngine.Serialization;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Scenes;

[Resource("scene", preferredExtension: ExtensionJson)]
public class Scene : Resource, ISelfSerializer
{
    public const string Extension = "foldscene";

    public readonly ComponentMap Components;

    public readonly IGameCore Core;
    public readonly EventMap Events;

    public readonly ResourceCollections Resources;
    public readonly SystemMap Systems;

    private List<long> _deletedIds = new List<long>();


    private bool _hasAnything;

    private long _nextEntityId;
    private List<bool> _reclaimableIds;

    [DoNotSerialize]
    public CameraOverrides CameraOverrides;

    public bool Paused = false;

    public Scene()
    {
        Core = FoldGame.Game.Core;
        Components = new ComponentMap(this);
        Events = new EventMap();
        Systems = new SystemMap(this);
        Resources = new ResourceCollections(Core.Resources);
    }

    public Scene(IGameCore core, string identifier)
    {
        Core = core;
        Components = new ComponentMap(this);
        Events = new EventMap();
        Systems = new SystemMap(this);
        Resources = new ResourceCollections(Core.Resources);

        Identifier = identifier;
    }

    public Matrix GizmoTransformMatrix { get; set; }
    public long MainCameraId { get; set; }

    public ref Transform MainCameraTransform
    {
        get
        {
            if (CameraOverrides != null)
                return ref CameraOverrides.Transform;
            return ref Components.GetComponent<Transform>(MainCameraId);
        }
    }

    /// <summary>
    ///     The speed at which the game is simulated
    /// </summary>
    public float TimeScale { get; set; }

    public void DeleteEntity(long entityId, bool reclaimable = false, bool recursively = false)
    {
        ref Hierarchical hierarchical = ref Components.GetComponent<Hierarchical>(entityId);
        if (hierarchical.HasParent)
        {
            hierarchical.MutableParent.RemoveChild(entityId);
        }

        foreach (long childId in hierarchical.GetChildren(true))
        {
            if (recursively)
            {
                DeleteEntity(childId, reclaimable, true);
            }
            else
            {
                Components.GetComponent<Hierarchical>(childId).SetParent(-1);
            }
        }

        Components.RemoveAllComponents(entityId);
        if (reclaimable)
        {
            if (_reclaimableIds == null)
            {
                _reclaimableIds = new List<bool>();
                for (int i = 0; i < _deletedIds.Count; i++) _reclaimableIds.Add(false);
            }

            _reclaimableIds.Add(true);
        }

        if (_deletedIds.Contains(entityId))
        {
            throw new InvalidOperationException("Entity id was already deleted!!!!");
        }

        _deletedIds.Add(entityId);
    }

    public long CreateEntityId(string name)
    {
        long newEntityId;
        if (_reclaimableIds != null && _reclaimableIds.Count > 0)
        {
            newEntityId = -1;
            for (int i = _reclaimableIds.Count - 1; i >= 0; i--)
            {
                if (!_reclaimableIds[i])
                {
                    newEntityId = _deletedIds[i] + (1L << 32);
                    _deletedIds.RemoveAt(i);
                    _reclaimableIds.RemoveAt(i);
                    break;
                }

                if (i == 0) newEntityId = _nextEntityId++;
            }

            if (newEntityId == -1) throw new InvalidOperationException();
        }
        else if (_deletedIds.Count > 0)
        {
            newEntityId = _deletedIds[_deletedIds.Count - 1] + (1L << 32);
            _deletedIds.RemoveAt(_deletedIds.Count - 1);
        }
        else
        {
            newEntityId = _nextEntityId++;
        }

        ref Hierarchical hierarchical = ref Components.CreateComponent<Hierarchical>(newEntityId);
        ref Transform transform = ref Components.CreateComponent<Transform>(newEntityId);
        // try {
        //     Components.GetComponent<Transform>(newEntityId);
        // } catch(Exception x) {
        //     FoldUtil.Breakpoint();
        // }
        Components.CreateComponent<EntityName>(newEntityId).Name = name;
        _hasAnything = true;
        // Console.WriteLine($"Created entity {newEntityId}");
        return newEntityId;
    }

    public Entity CreateEntity(string name = "Unnamed Entity")
    {
        return new Entity(this, CreateEntityId(name));
    }

    public virtual void Input()
    {
        Systems.InvokeInput();

        Flush();
    }

    public virtual void Update()
    {
        Access();

        Systems.InvokeFixedUpdate();
        Systems.InvokeUpdate();

        Core.CommandQueue.ExecuteAll();

        Flush();
    }

    public void Flush()
    {
        Systems.Flush();
        Components.Flush();
    }

    public void Render(IRenderingUnit renderer)
    {
        Systems.InvokeRender(renderer);
    }
    
    private static void WriteMatrix(Matrix mat)
    {
        Console.Write("| \t");
        Console.Write(mat.M11);
        Console.Write("\t");
        Console.Write(mat.M21);
        Console.Write("\t");
        Console.Write(mat.M31);
        Console.Write("\t");
        Console.Write(mat.M41);
        Console.Write("\t |\n");

        Console.Write("| \t");
        Console.Write(mat.M12);
        Console.Write("\t");
        Console.Write(mat.M22);
        Console.Write("\t");
        Console.Write(mat.M32);
        Console.Write("\t");
        Console.Write(mat.M42);
        Console.Write("\t |\n");

        Console.Write("| \t");
        Console.Write(mat.M13);
        Console.Write("\t");
        Console.Write(mat.M23);
        Console.Write("\t");
        Console.Write(mat.M33);
        Console.Write("\t");
        Console.Write(mat.M43);
        Console.Write("\t |\n");

        Console.Write("| \t");
        Console.Write(mat.M14);
        Console.Write("\t");
        Console.Write(mat.M24);
        Console.Write("\t");
        Console.Write(mat.M34);
        Console.Write("\t");
        Console.Write(mat.M44);
        Console.Write("\t |\n\n");
    }

    public void DrawGizmo(Vector2 from, Vector2 to, Color color, Color? colorTo = null, float zOrder = 0)
    {
        IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
        if (gizmoLayer != null)
        {
            Vector2 fromScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                Core.RenderingUnit.WorldLayer.CameraToLayer(from.ApplyMatrixTransform(GizmoTransformMatrix)),
                Core.RenderingUnit.GizmoLayer);
            Vector2 toScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                Core.RenderingUnit.WorldLayer.CameraToLayer(to.ApplyMatrixTransform(GizmoTransformMatrix)),
                Core.RenderingUnit.GizmoLayer);
            gizmoLayer.Surface.GizBatch.DrawLine(fromScreen, toScreen, color, colorTo, zOrder);
        }
    }

    public void DrawGizmo(Vector2 center, float radius, Color color, int sides = 24)
    {
        IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
        if (gizmoLayer != null)
        {
            Vector2 centerScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                Core.RenderingUnit.WorldLayer.CameraToLayer(center.ApplyMatrixTransform(GizmoTransformMatrix)),
                Core.RenderingUnit.GizmoLayer);
            Vector2 rightScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                Core.RenderingUnit.WorldLayer.CameraToLayer(
                    (center + Vector2.UnitX * radius).ApplyMatrixTransform(GizmoTransformMatrix)),
                Core.RenderingUnit.GizmoLayer);

            Complex current = rightScreen - centerScreen;
            Complex delta = Complex.FromRotation((float)(Math.PI * 2 / sides));
            for (int i = 0; i < sides; i++)
            {
                gizmoLayer.Surface.GizBatch.DrawLine((Complex)centerScreen + current,
                    (Complex)centerScreen + current * delta, color, color);
                current *= delta;
            }
        }
    }

    public void Serialize(SaveOperation writer)
    {
        Flush();
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            if (!writer.Options.Has(SerializeOnlyEntities.Instance))
            {
                c.WriteMember(nameof(_nextEntityId), _nextEntityId);
                c.WriteMember(nameof(_deletedIds), _deletedIds);
                c.WriteMember(nameof(Systems), (ISelfSerializer)Systems);
                if (writer.Options.Get(SerializeTempResources.Instance))
                    c.WriteMember(nameof(Resources), (ISelfSerializer)Resources);
            }

            c.WriteMember(nameof(Components), (ISelfSerializer)Components);
        });
    }

    public void Deserialize(LoadOperation reader)
    {
        Flush();
        bool resetIds = reader.Options.Has(DeserializeClearScene.Instance) || !_hasAnything;
        reader.ReadCompound(m =>
        {
            switch (m.Name)
            {
                case nameof(_nextEntityId):
                    if (resetIds) _nextEntityId = reader.Read<long>();
                    break;
                case nameof(_deletedIds):
                    if (resetIds) _deletedIds = reader.ReadList<long>();
                    break;
                case nameof(Systems):
                    reader.Deserialize(Systems);
                    break;
                case nameof(Components):
                    reader.Deserialize(Components);
                    break;
                case nameof(Resources):
                    reader.Deserialize(Resources);
                    break;
                default:
                    m.Skip();
                    break;
            }
        });
        _hasAnything = true;
    }

    public bool Reclaim(long entityId)
    {
        int deletedIndex = _deletedIds.IndexOf(entityId);
        if (deletedIndex > -1 && _reclaimableIds != null && _reclaimableIds[deletedIndex])
        {
            _deletedIds.RemoveAt(deletedIndex);
            _reclaimableIds.RemoveAt(deletedIndex);
            return true;
        }

        Console.WriteLine(
            $"Cannot reclaim entity ID {entityId}; Something external to the editor has already modified it.");

        return false;
    }

    public bool ReclaimAndCreate(long entityId, string name)
    {
        _hasAnything = true;
        if (Reclaim(entityId))
        {
            ref Hierarchical hierarchical = ref Components.CreateComponent<Hierarchical>(entityId);
            ref Transform transform = ref Components.CreateComponent<Transform>(entityId);
            Components.CreateComponent<EntityName>(entityId).Name = name;
            // Console.WriteLine($"Created entity {entityId}");
            return true;
        }

        return false;
    }

    public bool IsEditorAttached()
    {
        return Systems.Get<EditorBase>() != null;
    }
}

public class SerializeOnlyEntities : Field<List<long>>
{
    public static readonly SerializeOnlyEntities Instance = new SerializeOnlyEntities();
}

public class SerializeOnlyComponents : Field<List<Type>>
{
    public static readonly SerializeOnlyComponents Instance = new SerializeOnlyComponents();
}

public class SerializeTempResources : Field<bool>
{
    public static readonly SerializeTempResources Instance = new SerializeTempResources();
}

public class SerializeExcludeSystems : Field<List<Type>>
{
    public static readonly SerializeExcludeSystems Instance = new SerializeExcludeSystems();
}

public class DeserializeClearScene : Field<bool>
{
    public static readonly DeserializeClearScene Instance = new DeserializeClearScene();
}

public class DeserializeRemapIds : Field<EntityIdRemapper>
{
    public static readonly DeserializeRemapIds Instance = new DeserializeRemapIds();
}