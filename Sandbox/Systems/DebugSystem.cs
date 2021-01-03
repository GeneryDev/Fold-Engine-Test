using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sandbox.Components;

namespace Sandbox.Systems {
    [GameSystem("sandbox:test", ProcessingCycles.Input)]
    public class DebugSystem : GameSystem {
        private ComponentIterator<Living> _livingComponents;

        internal override void Initialize() {
            _livingComponents = Owner.Components.CreateIterator<Living>(IterationFlags.None);
        }

        private Vector2 _previousMouseWorldPos;
        
        public override void OnInput() {
            if(Mouse.GetState().LeftButton == ButtonState.Pressed) {
                _livingComponents.Reset();
                
                while(_livingComponents.Next()) {
                    ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

                    Vector2 currentMouseScreenPos = Mouse.GetState().Position.ToVector2();
                    Vector2 currentMouseWorldPos = RenderingLayer.ScreenToWorld(Owner.Controller.RenderingUnit.Layers["screen"],
                        currentMouseScreenPos);

                    currentMouseWorldPos = Owner.MainCameraTransform.Apply(currentMouseWorldPos);

                    transform.Position = currentMouseWorldPos;

                    if(_livingComponents.HasCoComponent<Physics>()) {
                        _livingComponents.GetCoComponent<Physics>().Velocity = (currentMouseWorldPos - _previousMouseWorldPos) * 4;
                    }
                    // Console.WriteLine(currentMouseWorldPos);

                    _previousMouseWorldPos = currentMouseWorldPos;
                }
                
            }
        }
    }
}