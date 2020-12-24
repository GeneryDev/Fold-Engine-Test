using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Components;
using FoldEngine.Interfaces;

using Microsoft.Xna.Framework;

using Woofer;

namespace FoldEngine.Scenes
{
    public class Scene
    {
        public readonly IGameController Controller;
        public string Name { get; set; } = "Scene";

        public ComponentMap Components;
        public SystemMap Systems;
        internal EntityObjectPool EntityObjectPool;

        private long _nextEntityId = 0;

        /// <summary>
        /// The speed at which the game is simulated
        /// </summary>
        public float TimeScale { get; set; }

        public Scene()
        {
            Components = new ComponentMap(this);
            Systems = new SystemMap(this);
            EntityObjectPool = new EntityObjectPool(this);
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
    }
}
