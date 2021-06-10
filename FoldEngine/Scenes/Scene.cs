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
using FoldEngine.Util;
using Microsoft.Xna.Framework;

using Woofer;

namespace FoldEngine.Scenes
{
    public class Scene
    {
        public readonly IGameCore Core;
        public string Name { get; set; } = "Scene";

        public readonly ComponentMap Components;
        public readonly SystemMap Systems;
        public readonly EventManager Events;
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
            Events = new EventManager(this);
            Systems = new SystemMap(this);
            EntityObjectPool = new EntityObjectPool(this);
            Meshes = new MeshCollection();
        }

        public long CreateEntityId(string name)
        {
            long newEntityId = _nextEntityId++;
            ref Transform transform = ref Components.CreateComponent<Transform>(newEntityId);
            Components.CreateComponent<EntityName>(newEntityId).Name = name;
            Console.WriteLine($"Created entity {newEntityId}");
            return newEntityId;
        }

        public Entity CreateEntity(string name = "Unnamed Entity")
        {
            return EntityObjectPool.GetOrCreateEntityObject(CreateEntityId(name));
        }
        

        bool _initialized = false;

        public virtual void Initialize()
        {
            
        }

        public virtual void Input()
        {
            Systems.InvokeInput();

            Systems.Flush();
            Components.Flush();
        }

        public virtual void Update()
        {
            if(!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            Systems.InvokeUpdate();

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

        public void DrawGizmo(Vector2 from, Vector2 to, Color color, Color? colorTo = null, float zOrder = 0) {
            IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
            if(gizmoLayer != null) {
                Vector2 fromScreen = RenderingLayer.WorldToScreen(gizmoLayer, from.ApplyMatrixTransform(GizmoTransformMatrix));
                Vector2 toScreen = RenderingLayer.WorldToScreen(gizmoLayer, to.ApplyMatrixTransform(GizmoTransformMatrix));
                gizmoLayer.Surface.GizBatch.DrawLine(fromScreen, toScreen, color, colorTo, zOrder);
            }
        }
        public void DrawGizmo(Vector2 center, float radius, Color color, int sides = 24) {
            IRenderingLayer gizmoLayer = Core.RenderingUnit.GizmoLayer;
            if(gizmoLayer != null) {
                Vector2 centerScreen = RenderingLayer.WorldToScreen(gizmoLayer, center.ApplyMatrixTransform(GizmoTransformMatrix));
                Vector2 rightScreen = RenderingLayer.WorldToScreen(gizmoLayer, (center + Vector2.UnitX*radius).ApplyMatrixTransform(GizmoTransformMatrix));

                Complex current = rightScreen - centerScreen;
                Complex delta = Complex.FromRotation((float) (Math.PI * 2 / sides));
                for(int i = 0; i < sides; i++) {
                    gizmoLayer.Surface.GizBatch.DrawLine((Complex) centerScreen + current, (Complex) centerScreen + current*delta , color, color);
                    current *= delta;
                }
            }
        }
    }
}
