﻿using System;
using FoldEngine.Commands;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Systems {

    [GameSystem("fold:editor.base", ProcessingCycles.All)]
    public class EditorBase : GameSystem {
        public const int SidebarX = 1280 * 4 / 5;
        public const int SidebarWidth = 1280 / 5;

        public const int SidebarMargin = 4;

        // public override bool ShouldSave => false;
        
        public GuiEnvironment Environment;
        
        private Type[] _modalTypes = new Type[] {typeof(EditorMenu), typeof(EditorEntitiesList), typeof(EditorSystemsList)};

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
            IRenderingLayer layer = renderer.MainGroup["screen"];
            Environment.Layer = layer;
            Rectangle sidebar = new Rectangle(SidebarX, 0, SidebarWidth, 720);
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
        }
    }
}