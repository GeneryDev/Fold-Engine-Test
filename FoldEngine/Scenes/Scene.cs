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
        internal readonly EntityObjectPool EntityObjectPool;

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
            EntityObjectPool = new EntityObjectPool(this);
            Meshes = new MeshCollection();
        }

        private List<long> _recycleQueue = new List<long>();

        public void Recycle(long entityId) {
            _recycleQueue.Add(entityId + (1L << 32));
            Components.RemoveAllComponents(entityId);
        }

        public long CreateEntityId(string name) {
            long newEntityId;
            if(_recycleQueue.Count > 0) {
                newEntityId = _recycleQueue[_recycleQueue.Count - 1];
                _recycleQueue.RemoveAt(_recycleQueue.Count - 1);
            } else {
                newEntityId = _nextEntityId++;
            }

            ref Transform transform = ref Components.CreateComponent<Transform>(newEntityId);
            Components.CreateComponent<EntityName>(newEntityId).Name = name;
            Console.WriteLine($"Created entity {newEntityId}");
            return newEntityId;
        }

        public Entity CreateEntity(string name = "Unnamed Entity") {
            return EntityObjectPool.GetOrCreateEntityObject(CreateEntityId(name));
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
                    c.WriteMember(nameof(_recycleQueue), _recycleQueue);
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
                    if(c.HasMember(nameof(_recycleQueue))) _recycleQueue = c.GetListMember<long>(nameof(_recycleQueue));
                }

                if(c.HasMember(nameof(Systems))) c.DeserializeMember(nameof(Systems), Systems);
                if(c.HasMember(nameof(Components))) c.DeserializeMember(nameof(Components), Components);
            });
        }
    }

    public class SerializeOnlyEntities : Field<List<long>> {
        public static SerializeOnlyEntities Instance = new SerializeOnlyEntities();
    }

    
    public class DeserializeClearScene : Field<bool> {
        public static DeserializeClearScene Instance = new DeserializeClearScene();
    }
    public class DeserializeRemapIds : Field<EntityIdRemapper> {
        public static DeserializeRemapIds Instance = new DeserializeRemapIds();
    }
}
