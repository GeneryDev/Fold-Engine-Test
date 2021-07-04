using FoldEngine.Scenes;
using FoldEngine.Util;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoldEngine.Editor.Inspector;

namespace FoldEngine.Components {
    [Component("fold:transform_2d")]
    [ComponentInitializer(typeof(Transform), nameof(InitializeComponent))]
    public struct Transform {
        /// <summary>
        /// Static unique null transform. Has no scene, or children
        /// </summary>
        private static Transform _nullTransform = default;

        /// <summary>
        /// Only false for null transforms
        /// Double negative was chosen such that default transforms have this as false by default, and thus can't be considered not-null
        /// </summary>
        [HideInInspector]
        public readonly bool IsNotNull;

        /// <summary>
        /// Reference to the scene this component belongs to.
        /// </summary>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// ID of the entity that owns this transform (-1 for null)
        /// </summary>
        [EntityId]
        private readonly long _ownerId;

        /// <summary>
        /// ID of the parent entity (-1 for null and transforms without parent)
        /// </summary>
        [HideInInspector]
        [EntityId]
        public long ParentId;

        /// <summary>
        /// Position in 2D space where this entity is located, relative to its parent
        /// </summary>
        [Name("Position")]
        public Vector2 LocalPosition;

        /// <summary>
        /// Complex number used to quickly multiply coordinates for rotations.
        /// </summary>
        private Complex _rotationComplex;

        /// <summary>
        /// Entity ID of this transform's first child (or -1 if it has no children)
        /// </summary>
        [HideInInspector]
        [EntityId]
        public long FirstChildId;

        /// <summary>
        /// Entity ID of this transform's previous sibling (or -1 if it's the first child)
        /// </summary>
        [HideInInspector]
        [EntityId]
        public long PreviousSiblingId;

        /// <summary>
        /// Entity ID of this transform's next sibling (or -1 if it's the last child)
        /// </summary>
        [HideInInspector]
        [EntityId]
        public long NextSiblingId;

        /// <summary>
        /// Float used to save this transform's rotation in radians. Used for the LocalRotation property
        /// </summary>
        [Name("Rotation")]
        public float _localRotation;

        /// <summary>
        /// This transform's local rotation, in radians.
        /// </summary>
        public float LocalRotation {
            get => _localRotation;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if(_localRotation != value) {
                    _localRotation = value;
                    _rotationComplex = Complex.FromRotation(value);
                }
            }
        }

        /// <summary>
        /// 2D vector for the scale of this entity
        /// </summary>
        [Name("Scale")]
        public Vector2 LocalScale;

        /// <summary>
        /// Returns an initialized transform component with all its correct default values.
        /// </summary>
        /// <param name="scene">The scene this transform is being created in</param>
        /// <param name="entityId">The ID of the entity this transform is being created for</param>
        /// <returns>An initialized transform component with all its correct default values.</returns>
        public static Transform InitializeComponent(Scene scene, long entityId) {
            return new Transform(entityId) {Scene = scene};
        }

        private Transform(long entityId) {
            IsNotNull = true;

            Scene = null;
            _ownerId = entityId;
            ParentId = -1;

            FirstChildId = -1;
            PreviousSiblingId = -1;
            NextSiblingId = -1;

            LocalPosition = Vector2.Zero;
            _rotationComplex = new Complex(1, 0);
            LocalScale = new Vector2(1, 1);

            _localRotation = 0;
        }

        /// <summary>
        /// Returns a read-only reference to this transform's parent, or RootTransform if this transform has no parent.
        /// </summary>
        public ref readonly Transform Parent {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(ParentId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(ParentId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Returns a mutable reference to this transform's parent. 
        /// The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of components.
        /// Only use this property when you need to change a property on the parent transform. Otherwise, use Parent.
        /// DO NOT assign a value to the returned reference.
        /// </summary>
        public ref Transform MutableParent {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(ParentId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(ParentId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Returns a read-only reference to this transform's next sibling, or RootTransform if this transform has no next sibling.
        /// </summary>
        public ref readonly Transform NextSibling {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(NextSiblingId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(NextSiblingId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Returns a mutable reference to this transform's next sibling. 
        /// The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of components.
        /// Only use this property when you need to change a property on the parent transform. Otherwise, use NextSibling.
        /// DO NOT assign a value to the returned reference.
        /// </summary>
        public ref Transform MutableNextSibling {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(NextSiblingId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(NextSiblingId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Returns a read-only reference to this transform's first child, or RootTransform if this transform has no first child.
        /// </summary>
        public ref readonly Transform FirstChild {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(FirstChildId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(FirstChildId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Returns a mutable reference to this transform's first child. 
        /// The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of components.
        /// Only use this property when you need to change a property on the parent transform. Otherwise, use FirstChild.
        /// DO NOT assign a value to the returned reference.
        /// </summary>
        public ref Transform MutableFirstChild {
            get {
                if(!IsNotNull) return ref _nullTransform;
                if(FirstChildId != -1) {
                    return ref Scene.Components.GetComponent<Transform>(FirstChildId);
                } else {
                    return ref _nullTransform;
                }
            }
        }

        /// <summary>
        /// Sets the given entity as this transform's parent, and adds this as a child of the given entity's transform.
        /// </summary>
        /// <param name="entity">The container entity that should become this transform's parent</param>
        public void SetParent(Entity entity) {
            SetParent(entity.EntityId);
        }

        /// <summary>
        /// Sets the given entity as this transform's parent, and adds this as a child of the given entity's transform.
        /// </summary>
        /// <param name="entityId">The ID of the entity that should become this transform's parent</param>
        public void SetParent(long entityId) {
            if(!IsNotNull) throw new InvalidOperationException();
            //TODO clearing children of old parent; ensuring root doesn't change.
            ParentId = entityId;
            AddChild(ref MutableParent, ref this);
        }

        /// <summary>
        /// Adds a child to the given parent.
        /// </summary>
        /// <param name="parent">The transform to which the given transform child should be added.</param>
        /// <param name="child">The child to be added to the parent.</param>
        private static void AddChild(ref Transform parent, ref Transform child) {
            if(!parent.IsNotNull) throw new InvalidOperationException();
            if(parent.FirstChildId == -1) {
                parent.FirstChildId = child._ownerId;
                return;
            }

            ref Transform currentChild = ref parent.Scene.Components.GetComponent<Transform>(parent.FirstChildId);
            while(currentChild.NextSiblingId != -1) {
                currentChild = ref parent.Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);
            }

            currentChild.NextSiblingId = child._ownerId;
            child.PreviousSiblingId = currentChild._ownerId;
        }

        public int ChildCount {
            get {
                if(FirstChildId == -1) {
                    return 0;
                }

                int count = 1;
                ref Transform currentChild = ref Scene.Components.GetComponent<Transform>(FirstChildId);
                while(currentChild.NextSiblingId != -1) {
                    currentChild = ref Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);
                    count++;
                }

                return count;
            }
        }

        public ComponentReference<Transform>[] Children {
            get {
                if(FirstChildId == -1) {
                    return new ComponentReference<Transform>[0];
                }

                ComponentReference<Transform>[] children = new ComponentReference<Transform>[ChildCount];

                ref Transform currentChild = ref Scene.Components.GetComponent<Transform>(FirstChildId);
                for(int i = 0; i < children.Length; i++) {
                    children[i] = new ComponentReference<Transform>(Scene, currentChild._ownerId);
                    if(i < children.Length - 1) {
                        currentChild = ref Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);
                    }
                }

                return children;
            }
        }


        /// <summary>
        /// This transform's absolute rotation, in radians.
        /// </summary>
        public float Rotation {
            get {
                if(!IsNotNull) return 0f;
                float total = _localRotation;
                ref readonly Transform current = ref Parent;
                while(current.IsNotNull) {
                    total += current._localRotation;
                    current = ref current.Parent;
                }

                return total;
            }
            set {
                if(!IsNotNull) throw new InvalidOperationException();
                LocalRotation = value - Parent.Rotation;
            }
        }

        /// <summary>
        /// This transform's absolute position in 2D space
        /// </summary>
        public Vector2 Position {
            get => this.Apply(Vector2.Zero);
            set {
                if(!IsNotNull) throw new InvalidOperationException();
                LocalPosition = (value - Parent.Apply(Vector2.Zero));
            }
        }

        public long EntityId => _ownerId;

        /// <summary>
        /// Applies this transformation to the given point in 2D space.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [Pure]
        public Vector2 Apply(Vector2 point) {
            if(!IsNotNull) return point;
            ref readonly Transform current = ref this;

            while(current.IsNotNull) {
                point = (Vector2) ((Complex) (point * current.LocalScale) * current._rotationComplex)
                        + current.LocalPosition;
                current = ref current.Parent;
            }

            return point;
        }

        /// <summary>
        /// Undoes this transformation from the given point in 2D space.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [Pure]
        public Vector2 ApplyReverse(Vector2 point) {
            if(!IsNotNull) return point;
            ref readonly Transform current = ref this;

            return (Vector2) ((Complex) (Parent.ApplyReverse(point) - current.LocalPosition) / current._rotationComplex)
                   / current.LocalScale;
        }

        public override string ToString() {
            return IsNotNull ? $"fold:transform_2d|{LocalPosition}" : "fold:transform_2d|NULL";
        }
    }
}
