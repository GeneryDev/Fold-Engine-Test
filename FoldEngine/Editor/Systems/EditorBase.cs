using System;
using FoldEngine.Commands;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Woofer;

namespace FoldEngine.Editor.Systems {

    [GameSystem("fold:editor.base", ProcessingCycles.All)]
    public class EditorBase : GameSystem {
        public const int SidebarX = 1280 * 4 / 5;
        public const int SidebarWidth = 1280 / 5;

        public const int SidebarMargin = 4;

        // public override bool ShouldSave => false;
        
        public GuiEnvironment Environment;
        
        private Type[] _modalTypes = new Type[] {typeof(EditorMenu), typeof(EditorEntitiesList), typeof(EditorSystemsList)};
        private bool _readjustViewport;

        public override void SubscribeToEvents() {
            Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => {
                _readjustViewport = true;
            });
        }

        internal override void Initialize() {
            Environment = new GuiEnvironment() {
                PerformAction = (int actionId, long data) => {
                    switch(actionId) {
                        case SceneEditor.Actions.ChangeToMenu: {
                            Console.WriteLine($"Change to view {data}");
                            Owner.Events.Invoke(new ForceModalChangeEvent(_modalTypes[(int)data]));
                            break;
                        }
                        case SceneEditor.Actions.Save: {
                            Console.WriteLine("Save");
                            break;
                        }
                        case SceneEditor.Actions.ExpandCollapseEntity: {
                            Owner.Systems.Get<EditorEntitiesList>().ExpandCollapseEntity(data);
                            break;
                        }
                        case SceneEditor.Actions.Test: {
                            Owner.Core.CommandQueue.Enqueue(new SetWindowSizeCommand(new Point(1920, 1040)));
                            break;
                        }
                    }
                }
            };
        }

        public override void OnInput() {
            Environment.Input(Owner.Core.InputUnit);
            

            if(Environment.MouseRight.Down) {
                
                Owner.Core.RenderingUnit.RootGroup.Dependencies[0].Destination.Location = Mouse.GetState().Position;
            }
        }

        public override void OnUpdate() {
            Environment.Update();
        }

        public override void OnRender(IRenderingUnit renderer) {
            IRenderingLayer layer = renderer.RootGroup["editor_gui"];
            Environment.Layer = layer;
            Rectangle sidebar = new Rectangle(layer.LayerSize.X - SidebarWidth, 0, SidebarWidth, layer.LayerSize.Y);
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = sidebar,
                Color = new Color(45, 45, 48)
            });
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = new Rectangle(sidebar.X + SidebarMargin, sidebar.Y + SidebarMargin, sidebar.Width - SidebarMargin*2, sidebar.Height - SidebarMargin*2),
                Color = new Color(37, 37, 38)
            });

            if(_readjustViewport) {
                renderer.Groups["editor"].Dependencies[0].Group.Size = new Point(renderer.WindowSize.X - SidebarWidth, renderer.WindowSize.Y);
                renderer.Groups["editor"].Dependencies[0].Destination = new Rectangle(0, 0, renderer.WindowSize.X - SidebarWidth, renderer.WindowSize.Y);
                _readjustViewport = false;
            }
        }
    }
}