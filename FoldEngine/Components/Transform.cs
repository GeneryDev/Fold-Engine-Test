using FoldEngine.Scenes;
using FoldEngine.Util;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Components
{
    [Component("fold:transform_2d")]
    public struct Transform
    {
        public Scene Scene;
        public long ParentId;

        //public ref Transform Parent => ParentId != -1 ? Scene.Components.GetComponent<Transform>(ParentId) : ref new Transform();

        public Vector2 LocalPosition;
        internal Complex RotationComplex;
        public Vector2 LocalScale;

        internal float _localRotation;
        public float LocalRotation
        {
            get
            {
                return _localRotation;
            }
            set
            {
                _localRotation = value;
                RotationComplex = Complex.FromRotation(value);
            }
        }

        /*public float Rotation
        {
            get
            {
                return _localRotation + (Parent?._localRotation ?? 0f);
            }
            set
            {
                float newLocalRotation = value;
                if(Parent != null)
                {
                    newLocalRotation -= Parent.Rotation;
                }
                LocalRotation = newLocalRotation;
            }
        }

        public Vector2 Position
        {
            get
            {
                return Apply(LocalPosition, Parent);
            }
            set
            {
                LocalPosition = (value - Apply(Vector2.Zero, Parent));
            }
        }

        private static Vector2 Apply(Vector2 point, Transform transform)
        {
            while(transform != null)
            {
                point = Apply((Vector2)((Complex)(point * transform.LocalScale) * transform.RotationComplex) + transform.LocalPosition, transform.Parent);
                transform = transform.Parent;
            }
            return point;
        }*/

        public override string ToString()
        {
            return $"fold:transform_2d|{LocalPosition}";
        }
    }
}
