using System;
using FoldEngine;
using FoldEngine.Audio;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Systems;
using FoldEngine.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sandbox.Components;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace Sandbox.Systems {
    [GameSystem("sandbox:test", ProcessingCycles.Input | ProcessingCycles.Render)]
    [Listening(typeof(CollisionEvent))]
    public class DebugSystem : GameSystem {
        private ComponentIterator<Living> _livingComponents;

        internal override void Initialize() {
            _livingComponents = Owner.Components.CreateIterator<Living>(IterationFlags.None);
        }

        private Vector2 _previousMouseWorldPos;

        private bool _previouslyPressed = false;
        private Vector2 _leftPos;
        private Vector2 _leftVel;
        
        public override void OnInput() {
            bool jump = Owner.Core.InputUnit.Players[0].Get<ButtonAction>("movement.jump").Consume();
            float moveX = Owner.Core.InputUnit.Players[0].Get<AnalogAction>("movement.axis.x");
            if(Owner.Core.InputUnit.Players[0].Get<ButtonAction>("movement.sprint").Pressed) {
                moveX *= 2;
            }

            if(jump) {
                SoundInstance soundInstance = Owner.Core.AudioUnit.CreateInstance("Audio/failure");
                soundInstance.Pan = MathHelper.Clamp(moveX, -1, 1);
                soundInstance.PlayOnce();
            }
            
            _livingComponents.Reset();
                
            while(_livingComponents.Next()) {
                if(_livingComponents.HasCoComponent<Physics>()) {
                    if(jump) {
                        _livingComponents.GetCoComponent<Physics>().Velocity.Y = 8;
                    }
                    _livingComponents.GetCoComponent<Physics>().Velocity.X = 2*moveX;
                }
            }
            
            if(Mouse.GetState().LeftButton == ButtonState.Pressed) {
                _livingComponents.Reset();
                
                while(_livingComponents.Next()) {
                    ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

                    Vector2 currentMouseScreenPos = Mouse.GetState().Position.ToVector2();
                    Vector2 currentMouseWorldPos = RenderingLayer.ScreenToWorld(Owner.Core.RenderingUnit.Layers["screen"],
                        currentMouseScreenPos);

                    currentMouseWorldPos = Owner.MainCameraTransform.Apply(currentMouseWorldPos);

                    _leftPos = currentMouseWorldPos;
                    transform.Position = currentMouseWorldPos;

                    if(_livingComponents.HasCoComponent<Physics>()) {
                        _leftVel = (currentMouseWorldPos - _previousMouseWorldPos) * 4;
                        _livingComponents.GetCoComponent<Physics>().Velocity = _leftVel;
                    }
                    // Console.WriteLine(currentMouseWorldPos);

                    _previousMouseWorldPos = currentMouseWorldPos;
                }

                _previouslyPressed = true;
            } else if(_previouslyPressed) {
                Console.WriteLine($"Released at {_leftPos} with velocity {_leftVel}");
                _previouslyPressed = false;
            }

            if(Mouse.GetState().RightButton == ButtonState.Pressed) {
                _livingComponents.Reset();
                
                while(_livingComponents.Next()) {
                    ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

                    // transform.Position = new Vector2(6.65625f, -6.875f);
                    // transform.Position = new Vector2(-4.734375f, -6f);
                    transform.Position = new Vector2(-4.75f, -6.015625f);

                    if(_livingComponents.HasCoComponent<Physics>()) {
                        // _livingComponents.GetCoComponent<Physics>().Velocity = new Vector2(-2.125f, 5.125f);
                        _livingComponents.GetCoComponent<Physics>().Velocity = new Vector2(0, 0);
                    }
                }
            }
        }

        private RenderedText _renderedHelloWorld;
        private Vector2 _lastNormal;

        public override void OnRender(IRenderingUnit renderer) {
            if(!_renderedHelloWorld.HasValue) {
                renderer.Fonts["default"].RenderString("Hello World!\nBut the world doesn't say hello back", out _renderedHelloWorld);
            }
            _renderedHelloWorld.DrawOnto(renderer.Layers["screen"].Surface, new Point(0, 2*8 * 3), Color.LightGray, 2);
            
            renderer.Fonts["default"].DrawString($"FPS:{Time.FramesPerSecond}", renderer.Layers["screen"].Surface, new Point(0, 2*8), Color.Yellow, 2);
            renderer.Fonts["default"].DrawString($"Normal:{_lastNormal}", renderer.Layers["screen"].Surface, new Point(0, 2*8 * 6), Color.Yellow, 2);
        }

        public override void EventFired(object sender, Event e) {
            if(e is CollisionEvent collision) {
                _lastNormal = collision.Normal;
                // Console.WriteLine(collision.Normal);
            }
        }
    }
}