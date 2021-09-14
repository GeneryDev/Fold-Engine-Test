using System;
using EntryProject.Editor.Gui.Hierarchy;
using EntryProject.Util;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui.Hierarchy {
    public class HierarchyElement : GuiLabel {
        
        private PooledValue<IGuiAction> _expandAction;
        private PooledValue<IGuiAction> _leftAction;
        private PooledValue<IGuiAction> _rightAction;
        private PooledValue<HierarchyDragOntoAction> _dragOntoAction;
        
        protected virtual Color NormalColor => new Color(37, 37, 38);
        protected virtual Color RolloverColor => new Color(63, 63, 70);
        protected virtual Color PressedColor => new Color(63, 63, 70);
        
        protected virtual Color SelectedColor => Color.CornflowerBlue;

        internal IHierarchy _hierarchy;
        
        protected bool _dragging = false;
        protected int _depth = 0;
        protected bool _expandable = false; 
        protected bool _expanded = false;
        protected bool _selected = false;

        protected Rectangle ExpandBounds;

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            FontSize(14);
            TextAlignment(-1);
            _selected = false;
            _hierarchy = null;
            _expandAction.Free();
            _leftAction.Free();
            _rightAction.Free();
        }

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 0;
            ExpandBounds = new Rectangle(Bounds.Left + 4 + 16 * (_depth),
                Bounds.Top + Bounds.Height / 2 - 16 / 2, 16,
                16);
        }

        public HierarchyElement Entity(Entity entity, int depth = 0) {
            Text(entity.Name);
            TextMargin(4 + 16 * (depth+1) + 4);
            _expandable = entity.Transform.FirstChildId != -1;
            _depth = depth;
            ExpandAction<ExpandCollapseEntityAction>().Id(entity.EntityId);
            LeftAction<SelectEntityAction>().Id(entity.EntityId);
            RightAction<ShowEntityContextMenu>().Id(entity.EntityId);
            DragOntoAction<HierarchyDragOntoAction<long>>().Id(entity.EntityId);
            return this;
        }

        public new HierarchyElement Icon(ITexture icon) {
            base.Icon(icon);
            return this;
        }

        public new HierarchyElement Icon(ITexture icon, Color color) {
            base.Icon(icon, color);
            return this;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
                Environment.HoverTarget.Hierarchy = _hierarchy;
            }
            
            if(Pressed(MouseEvent.LeftButton) && Environment.HoverTargetPrevious.Element != this) {
                _dragging = true;
                if(_hierarchy != null) _hierarchy.Dragging = true;
            }

            if(_dragging) {
                if(Parent.Environment is EditorEnvironment editorEnvironment) {
                    if(!editorEnvironment.DraggingElements.Contains(this)) {
                        editorEnvironment.DraggingElements.Add(this);
                    }
                }
            }

            // if(_dragging) {
            //     offset += Parent.Environment.MousePos - Bounds.Center;
            // }

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = _selected ? SelectedColor : Pressed(MouseEvent.LeftButton) ? PressedColor : Rollover ? RolloverColor : NormalColor,
                DestinationRectangle = Bounds.Translate(offset)
            });

            if(_expandable) {
                ITexture triangleTexture =
                    renderer.Textures[_expanded ? "editor:triangle.down" : "editor:triangle.right"];
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = triangleTexture,
                    DestinationRectangle = ExpandBounds.Translate(offset)
                });
            }
            
            base.Render(renderer, layer, offset);
            
            if(_hierarchy != null && _hierarchy.Dragging && Environment.HoverTarget.Element == this) {
                int relative = 0;
                if(Environment.MousePos.Y <= Bounds.Top + Bounds.Height / 3) {
                    relative = -1;
                } else if(Environment.MousePos.Y > Bounds.Bottom - Bounds.Height / 3) {
                    relative = 1;
                }
                _dragOntoAction.Value?.Relative(relative).Perform(this, default);

                if(relative != 0) {
                    int lineCenterY = Bounds.Center.Y + (Bounds.Height / 2) * relative;
                    int x = 4 + 16 * (_depth + 1) + 4;
                    Rectangle lineBounds = new Rectangle(Bounds.Left + x, lineCenterY, Bounds.Width - x, 0);
                    _hierarchy.DragLine = lineBounds;
                }
            }
            
            if(_dragging) {
                _hierarchy?.DrawDragLine(renderer, layer);
            }
        }

        public HierarchyElement ExpandAction(IGuiAction action) {
            _expandAction.Value = action;
            return this;
        }

        public T ExpandAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _expandAction.Value = action;
            return action;
        }

        public HierarchyElement LeftAction(IGuiAction action) {
            _leftAction.Value = action;
            return this;
        }

        public T LeftAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _leftAction.Value = action;
            return action;
        }

        public HierarchyElement RightAction(IGuiAction action) {
            _rightAction.Value = action;
            return this;
        }

        public T RightAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _rightAction.Value = action;
            return action;
        }

        public HierarchyElement DragOntoAction(HierarchyDragOntoAction action) {
            _dragOntoAction.Value = action;
            return this;
        }

        public T DragOntoAction<T>() where T : HierarchyDragOntoAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _dragOntoAction.Value = action;
            return action;
        }
        
        public override void OnMouseReleased(ref MouseEvent e) {
            if(_dragging && e.Button == MouseEvent.LeftButton) {
                _dragging = false;
                if(_hierarchy != null) _hierarchy.Dragging = false;
                if(Parent.Environment is EditorEnvironment editorEnvironment) {
                    editorEnvironment.DraggingElements.Clear();
                }
            }
            if(Bounds.Contains(e.Position)) {
                switch(e.Button) {
                    case MouseEvent.LeftButton: {
                        if(_expandable && ExpandBounds.Contains(e.Position)) {
                            _expandAction.Value?.Perform(this, e);
                        } else {
                            _leftAction.Value?.Perform(this, e);
                        }
                        break;
                    }
                    case MouseEvent.RightButton: {
                        _rightAction.Value?.Perform(this, e);
                        break;
                    }
                }
            }
        }

        public HierarchyElement Expanded(bool expanded) {
            _expanded = expanded;
            return this;
        }

        public HierarchyElement Selected(bool selected) {
            _selected = selected;
            return this;
        }

        public HierarchyElement Hierarchy(IHierarchy hierarchy) {
            _hierarchy = hierarchy;
            return this;
        }
    }

    public abstract class HierarchyDragOntoAction : IGuiAction {
        protected int _relative = 0;

        public HierarchyDragOntoAction Relative(int relative) {
            _relative = relative;
            return this;
        }
        
        public IObjectPool Pool { get; set; }
        public abstract void Perform(GuiElement element, MouseEvent e);
    }

    public class HierarchyDragOntoAction<T> : HierarchyDragOntoAction {
        protected T _id;

        public HierarchyDragOntoAction<T> Id(T id) {
            _id = id;
            return this;
        }

        public override void Perform(GuiElement element, MouseEvent e) {
            if(element is HierarchyElement hierarchyElement) {
                if(hierarchyElement._hierarchy is Hierarchy<T> hierarchy) {
                    hierarchy.DragTargetId = _id;
                    hierarchy.DragRelative = _relative;
                }
            }
        }
    }
}