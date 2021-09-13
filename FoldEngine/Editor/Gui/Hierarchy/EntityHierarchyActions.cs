using System;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Views {
    public class ExpandCollapseEntityAction : IGuiAction {
        public IObjectPool Pool { get; set; }
        
        private long _id;
        public ExpandCollapseEntityAction Id(long id) {
            _id = id;
            return this;
        }
        
        public void Perform(GuiElement element, MouseEvent e) {
            if(element.Parent.Environment is EditorEnvironment editorEnvironment) {
                editorEnvironment.GetView<EditorHierarchyView>().ExpandCollapseEntity(_id);
            }
        }
    }
    public class SelectEntityAction : IGuiAction {
        private long _id;
        private int _depth;

        public SelectEntityAction Id(long id) {
            _id = id;
            return this;
        }

        public void Perform(GuiElement element, MouseEvent e) {
            if(element.Parent.Environment is EditorEnvironment editorEnvironment) {
                var editorBase = editorEnvironment.Scene.Systems.Get<EditorBase>();
                bool wasSelected = editorBase.EditingEntity.Contains(_id);

                bool control = editorEnvironment.Scene.Core.InputUnit.Devices.Keyboard[Keys.LeftControl].Down
                               || editorEnvironment.Scene.Core.InputUnit.Devices.Keyboard[Keys.RightControl].Down;
                
                bool shift = editorEnvironment.Scene.Core.InputUnit.Devices.Keyboard[Keys.LeftShift].Down
                               || editorEnvironment.Scene.Core.InputUnit.Devices.Keyboard[Keys.RightShift].Down;
                
                if(control) {
                    if(wasSelected) {
                        editorBase.EditingEntity.Remove(_id);
                    } else {
                        editorBase.EditingEntity.Add(_id);
                    }
                } else {
                    editorBase.EditingEntity.Clear();
                    editorBase.EditingEntity.Add(_id);
                }
                
                editorEnvironment.SwitchToView(editorEnvironment.GetView<EditorInspectorView>());
            }
        }

        public IObjectPool Pool { get; set; }
    }
    

    public class ShowEntityContextMenu : IGuiAction {
        private long _id;
        
        public ShowEntityContextMenu Id(long id) {
            _id = id;
            return this;
        }

        public void Perform(GuiElement element, MouseEvent e) {
            var contextMenu = element.Parent.Environment.ContextMenu;
            contextMenu.Reset(e.Position);
            contextMenu.Button("Edit", 14).LeftAction<DebugAction>();
            contextMenu.Button("Rename", 14).LeftAction<DebugAction>();
            
            contextMenu.Button("Create Child", 14).LeftAction<CreateEntityAction>().Id(_id);
            
            contextMenu.Button("Delete", 14).LeftAction<DeleteEntityAction>().Id(_id);
            contextMenu.Show();
        }
        
        public IObjectPool Pool { get; set; }
    }
}