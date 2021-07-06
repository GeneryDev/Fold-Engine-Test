using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Serialization;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

using Woofer;

namespace FoldEngine.Scenes
{
    public class Scene {
        public readonly IGameCore Core;
        public string Name { get; set; } = "Scene";

        public readonly ComponentMap Components;
        public readonly EventMap Events;
        public readonly SystemMap Systems;

        public readonly MeshCollection Meshes;

        private long _nextEntityId = 0;

        public Matrix GizmoTransformMatrix { get; set; }
        public long MainCameraId { get; set; }

        public ref Transform MainCameraTransform => ref Components.GetComponent<Transform>(MainCameraId);

        /// <summary>
        /// The speed at which the game is simulated
        /// </summary>
        public float TimeScale { get; set; }

        public Scene(IGameCore core) {
            Core = core;
            Components = new ComponentMap(this);
            Events = new EventMap(this);
            Systems = new SystemMap(this);
            Meshes = new MeshCollection();
        }

        private List<long> _deletedIds = new List<long>();
        private List<bool> _reclaimableIds;

        public void DeleteEntity(long entityId, bool reclaimable = false) {
            Components.RemoveAllComponents(entityId);
            if(reclaimable) {
                if(_reclaimableIds == null) {
                    _reclaimableIds = new List<bool>();
                    for(int i = 0; i < _deletedIds.Count; i++) {
                        _reclaimableIds.Add(false);
                    }
                }
                _reclaimableIds.Add(true);
            }
            _deletedIds.Add(entityId);
        }

        public long CreateEntityId(string name) {
            long newEntityId;
            if(_reclaimableIds != null && _reclaimableIds.Count > 0) {
                newEntityId = -1;
                for(int i = _reclaimableIds.Count - 1; i >= 0; i--) {
                    if(!_reclaimableIds[i]) {
                        newEntityId = _deletedIds[i] + (1L << 32);
                        _deletedIds.RemoveAt(i);
                        _reclaimableIds.RemoveAt(i);
                        break;
                    }

                    if(i == 0) {
                        newEntityId = _nextEntityId++;
                    }
                }

                if(newEntityId == -1) {
                    throw new InvalidOperationException();
                }
            } else if(_deletedIds.Count > 0) {
                newEntityId = _deletedIds[_deletedIds.Count - 1] + (1L << 32);
                _deletedIds.RemoveAt(_deletedIds.Count - 1);
            } else {
                newEntityId = _nextEntityId++;
            }

            ref Transform transform = ref Components.CreateComponent<Transform>(newEntityId);
            Components.CreateComponent<EntityName>(newEntityId).Name = name;
            Console.WriteLine($"Created entity {newEntityId}");
            return newEntityId;
        }

        public Entity CreateEntity(string name = "Unnamed Entity") {
            return new Entity(this, CreateEntityId(name));
        }


        bool _initialized = false;

        public virtual void Initialize() { }

        public virtual void Input() {
            Systems.InvokeInput();

            Systems.Flush();
            Components.Flush();
        }

        public virtual void Update() {
            if(!_initialized) {
                Initialize();
                _initialized = true;
            }

            Systems.InvokeUpdate();

            Core.CommandQueue.ExecuteAll();
            
            Systems.Flush();
            Components.Flush();
        }

        public void Render(IRenderingUnit renderer) {
            Systems.InvokeRender(renderer);
        }

        private static void WriteMatrix(Matrix mat) {
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

        public void DrawGizmo(Vector2 from, Vector2 to, Color color, Color? colorTo = null, float zOrder = 0) {
            IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
            if(gizmoLayer != null) {
                Vector2 fromScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                    Core.RenderingUnit.WorldLayer.CameraToLayer(from.ApplyMatrixTransform(GizmoTransformMatrix)),
                    Core.RenderingUnit.GizmoLayer);
                Vector2 toScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                    Core.RenderingUnit.WorldLayer.CameraToLayer(to.ApplyMatrixTransform(GizmoTransformMatrix)),
                    Core.RenderingUnit.GizmoLayer);
                gizmoLayer.Surface.GizBatch.DrawLine(fromScreen, toScreen, color, colorTo, zOrder);
            }
        }

        public void DrawGizmo(Vector2 center, float radius, Color color, int sides = 24) {
            IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
            if(gizmoLayer != null) {
                Vector2 centerScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                    Core.RenderingUnit.WorldLayer.CameraToLayer(center.ApplyMatrixTransform(GizmoTransformMatrix)),
                    Core.RenderingUnit.GizmoLayer);
                Vector2 rightScreen = Core.RenderingUnit.WorldLayer.LayerToLayer(
                    Core.RenderingUnit.WorldLayer.CameraToLayer((center + Vector2.UnitX * radius).ApplyMatrixTransform(GizmoTransformMatrix)),
                    Core.RenderingUnit.GizmoLayer);

                Complex current = rightScreen - centerScreen;
                Complex delta = Complex.FromRotation((float) (Math.PI * 2 / sides));
                for(int i = 0; i < sides; i++) {
                    gizmoLayer.Surface.GizBatch.DrawLine((Complex) centerScreen + current,
                        (Complex) centerScreen + current * delta, color, color);
                    current *= delta;
                }
            }
        }

        public void Save(string path) {
            var saveOp = new SaveOperation(path);
            Save(saveOp);
            saveOp.Close();
            saveOp.Dispose();
        }

        public void Save(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {

                if(!writer.Options.Has(SerializeOnlyEntities.Instance)) {
                    c.WriteMember(nameof(Name), Name);
                    c.WriteMember(nameof(_nextEntityId), _nextEntityId);
                    c.WriteMember(nameof(_deletedIds), _deletedIds);
                    c.WriteMember(nameof(Systems), (ISelfSerializer) Systems);
                }

                c.WriteMember(nameof(Components), (ISelfSerializer) Components);
            });
        }

        public void Load(string path) {
            var loadOp = new LoadOperation(path);
            Load(loadOp);
            loadOp.Close();
            loadOp.Dispose();
        }

        public void Load(LoadOperation reader) {
            reader.ReadCompound(c => {

                if(reader.Options.Has(DeserializeClearScene.Instance)) {
                    if(c.HasMember(nameof(Name))) Name = c.GetMember<string>(nameof(Name));
                    if(c.HasMember(nameof(_nextEntityId))) _nextEntityId = c.GetMember<long>(nameof(_nextEntityId));
                    if(c.HasMember(nameof(_deletedIds))) _deletedIds = c.GetListMember<long>(nameof(_deletedIds));
                }

                if(c.HasMember(nameof(Systems))) c.DeserializeMember(nameof(Systems), Systems);
                if(c.HasMember(nameof(Components))) c.DeserializeMember(nameof(Components), Components);
            });
        }

        public bool Reclaim(long entityId) {
            int deletedIndex = _deletedIds.IndexOf(entityId);
            if(deletedIndex > -1 && _reclaimableIds != null && _reclaimableIds[deletedIndex]) {
                _deletedIds.RemoveAt(deletedIndex);
                _reclaimableIds.RemoveAt(deletedIndex);
                return true;
            }

            Console.WriteLine($"Cannot reclaim entity ID {entityId}; Something external to the editor has already modified it.");

            return false;
        }

        public bool ReclaimAndCreate(long entityId, string name) {
            if(Reclaim(entityId)) {
                ref Transform transform = ref Components.CreateComponent<Transform>(entityId);
                Components.CreateComponent<EntityName>(entityId).Name = name;
                Console.WriteLine($"Created entity {entityId}");
                return true;
            }

            return false;
        }
    }

    public class SerializeOnlyEntities : Field<List<long>> {
        public static readonly SerializeOnlyEntities Instance = new SerializeOnlyEntities();
    }
    public class SerializeOnlyComponents : Field<List<Type>> {
        public static readonly SerializeOnlyComponents Instance = new SerializeOnlyComponents();
    }

    
    public class DeserializeClearScene : Field<bool> {
        public static readonly DeserializeClearScene Instance = new DeserializeClearScene();
    }
    public class DeserializeRemapIds : Field<EntityIdRemapper> {
        public static readonly DeserializeRemapIds Instance = new DeserializeRemapIds();
    }
}
