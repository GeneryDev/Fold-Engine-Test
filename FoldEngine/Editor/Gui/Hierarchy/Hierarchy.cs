using System.Collections.Generic;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace EntryProject.Editor.Gui.Hierarchy {
    public enum HierarchyDropMode {
        Before,
        Inside,
        FirstInside,
        After
    }

    public interface IHierarchy {
        GuiEnvironment Environment { get; set; }
        bool Pressed { get; set; }
        bool Dragging { get; set; }
        Rectangle DragLine { get; set; }
        void DrawDragLine(IRenderingUnit renderer, IRenderingLayer environmentBaseLayer);
    }

    public class Hierarchy<T> : IHierarchy {
        public int DragRelative = 0;
        public T DragTargetId;

        public List<T> Expanded = new List<T>();
        public List<T> Selected = new List<T>();

        public Hierarchy(GuiEnvironment environment) {
            Environment = environment;
            DragTargetId = DefaultId;
        }

        public Hierarchy(GuiPanel parent) {
            Environment = parent.Environment;
        }

        public virtual T DefaultId { get; } = default;
        public virtual bool CanDrag { get; set; } = true;
        public virtual bool CanDragInto { get; set; } = true;
        public GuiEnvironment Environment { get; set; }
        public bool Pressed { get; set; }
        public bool Dragging { get; set; }
        public Rectangle DragLine { get; set; }

        public void DrawDragLine(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Dragging && Environment.HoverTarget.Hierarchy == this && DragRelative != 0) {
                int thickness = 2;
                int arrowSpan = 5;
                int arrowLength = 8;
                Rectangle dragLineThick = DragLine;
                dragLineThick.Y -= thickness / 2;
                dragLineThick.Height = thickness;

                layer.Surface.Draw(new DrawRectInstruction {
                    Texture = renderer.WhiteTexture,
                    Color = Color.Coral,
                    DestinationRectangle = dragLineThick
                });
                layer.Surface.Draw(new DrawTriangleInstruction {
                    Texture = renderer.WhiteTexture,
                    Color = Color.Coral,
                    A = new Vector3(DragLine.Left, DragLine.Y - arrowSpan, 0),
                    B = new Vector3(DragLine.Left + arrowLength, DragLine.Y, 0),
                    C = new Vector3(DragLine.Left, DragLine.Y + arrowSpan, 0)
                });
            }
        }

        public void ExpandCollapse(T id) {
            if(!Expanded.Remove(id)) Expanded.Add(id);
        }

        public bool IsSelected(T id) {
            return Selected.Contains(id);
        }

        public bool IsExpanded(T id) {
            return Expanded.Contains(id);
        }

        public virtual void Drop() { }
    }
}