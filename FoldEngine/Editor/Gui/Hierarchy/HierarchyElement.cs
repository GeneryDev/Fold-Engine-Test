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
    public class HierarchyElement<TI> : GuiLabel {
        
        private PooledValue<IGuiAction> _expandAction;
        private PooledValue<IGuiAction> _leftDownAction;
        private PooledValue<IGuiAction> _leftUpAction;
        private PooledValue<IGuiAction> _rightAction;
        
        protected virtual Color NormalColor => new Color(37, 37, 38);
        protected virtual Color RolloverColor => new Color(63, 63, 70);
        protected virtual Color PressedColor => new Color(63, 63, 70);
        
        protected virtual Color SelectedColor => Color.CornflowerBlue;

        internal Hierarchy<TI> _hierarchy;
        
        protected TI _id = default;
        
        protected bool _dragging = false;
        protected int _depth = 0;
        protected bool _expandable = false; 
        protected bool _selected = false;

        protected Rectangle ExpandBounds;

        public override void Reset(GuiPanel parent) {
            base.Reset(parent);
            FontSize(14);
            TextAlignment(-1);
            _selected = false;
            _hierarchy = null;
            _id = default;
            _expandAction.Free();
            _leftDownAction.Free();
            _leftUpAction.Free();
            _rightAction.Free();
        }

        public override void AdjustSpacing(GuiPanel parent) {
            base.AdjustSpacing(parent);
            Margin = 0;
            ExpandBounds = new Rectangle(Bounds.Left + 4 + 16 * (_depth),
                Bounds.Top + Bounds.Height / 2 - 16 / 2, 16,
                16);
        }

        public HierarchyElement<TI> Entity(Entity entity, int depth = 0) {
            if(entity.EntityId is TI id) {
                Id(id);
                
                Text(entity.Name);
                TextMargin(4 + 16 * (depth+1) + 4);
                _expandable = entity.Transform.FirstChildId != -1;
                _depth = depth;
                ExpandAction<ExpandCollapseEntityAction>().Id(entity.EntityId);
                LeftDownAction<SelectEntityDownAction>().Id(entity.EntityId);
                LeftUpAction<SelectEntityUpAction>().Id(entity.EntityId);
                RightAction<ShowEntityContextMenu>().Id(entity.EntityId);
            } else {
                throw new ArgumentException("Cannot run HierarchyElement.Entity on an element whose ID Type is not long");
            }
            return this;
        }

        public HierarchyElement<TI> Id(TI id) {
            _id = id;
            return this;
        }

        public new HierarchyElement<TI> Icon(ITexture icon) {
            base.Icon(icon);
            return this;
        }

        public new HierarchyElement<TI> Icon(ITexture icon, Color color) {
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
                Color = _selected ? SelectedColor :
                    Pressed(MouseEvent.LeftButton) ? PressedColor :
                    Rollover ? RolloverColor : NormalColor,
                DestinationRectangle = Bounds.Translate(offset)
            });

            if(_expandable) {
                ITexture triangleTexture =
                    renderer.Textures[(_hierarchy?.IsExpanded(_id) ?? false) ? "editor:triangle.down" : "editor:triangle.right"];
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
                _hierarchy.DragTargetId = _id;
                _hierarchy.DragRelative = relative;

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

        public HierarchyElement<TI> ExpandAction(IGuiAction action) {
            _expandAction.Value = action;
            return this;
        }

        public T ExpandAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _expandAction.Value = action;
            return action;
        }

        public HierarchyElement<TI> LeftDownAction(IGuiAction action) {
            _leftDownAction.Value = action;
            return this;
        }

        public T LeftDownAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _leftDownAction.Value = action;
            return action;
        }

        public HierarchyElement<TI> LeftUpAction(IGuiAction action) {
            _leftUpAction.Value = action;
            return this;
        }

        public T LeftUpAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _leftUpAction.Value = action;
            return action;
        }

        public HierarchyElement<TI> RightAction(IGuiAction action) {
            _rightAction.Value = action;
            return this;
        }

        public T RightAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _rightAction.Value = action;
            return action;
        }

        public override void OnMousePressed(ref MouseEvent e) {
            _hierarchy.Pressed = true;
            if(e.Button == MouseEvent.LeftButton) {
                if(_expandable && ExpandBounds.Contains(e.Position)) {
                } else {
                    _leftDownAction.Value?.Perform(this, e);
                }
            }
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            _hierarchy.Pressed = false;
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
                            _leftUpAction.Value?.Perform(this, e);
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

        public HierarchyElement<TI> Selected(bool selected) {
            _selected = selected;
            return this;
        }

        public HierarchyElement<TI> Hierarchy(Hierarchy<TI> hierarchy) {
            _hierarchy = hierarchy;
            return this;
        }
    }
}