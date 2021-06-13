using System;
using System.IO;
using FoldEngine;
using FoldEngine.Audio;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
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
        
        private static readonly string TargetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Fold", "scenes");
        
        public override void OnInput() {
            float moveX = Owner.Core.InputUnit.Players[0].Get<AnalogAction>("movement.axis.x");
            if(Owner.Core.InputUnit.Players[0].Get<ButtonAction>("movement.sprint").Down) {
                moveX *= 2;
            }
            
            if(Owner.Core.InputUnit.Players[0].Get<ChangeAction>("zoom.in")) {
                Owner.MainCameraTransform.LocalScale /= 1.2f;
                Console.WriteLine("Scale is now: " + Owner.MainCameraTransform.LocalScale.X);
            } else if(Owner.Core.InputUnit.Players[0].Get<ChangeAction>("zoom.out")) {
                Owner.MainCameraTransform.LocalScale *= 1.2f;
                Console.WriteLine("Scale is now: " + Owner.MainCameraTransform.LocalScale.X);
            }


            if(Owner.Core.InputUnit.Players[0].Get<ButtonAction>("quicksave").Consume()) {
                Directory.CreateDirectory(TargetDirectory);
                Owner.Save(Path.Combine(TargetDirectory, "scene.foldscene"));
            } else if(Owner.Core.InputUnit.Players[0].Get<ButtonAction>("quickload").Consume()) {
                Owner.Load(Path.Combine(TargetDirectory, "scene.foldscene"));
            }
            
            _livingComponents.Reset();
                
            while(_livingComponents.Next()) {
                if(_livingComponents.HasCoComponent<Physics>()) {
                    if(_livingComponents.GetComponent().Grounded && Owner.Core.InputUnit.Players[0].Get<ButtonAction>("movement.jump").Consume()) {
                        SoundInstance soundInstance = Owner.Core.AudioUnit.CreateInstance("Audio/failure");
                        soundInstance.Pan = MathHelper.Clamp(moveX, -1, 1);
                        soundInstance.PlayOnce();
                        _livingComponents.GetCoComponent<Physics>().Velocity.Y = 8;
                    }
                    _livingComponents.GetCoComponent<Physics>().Velocity.X = 2*moveX;
                }
                _livingComponents.GetComponent().Grounded = false;
            }
            
            // if(Mouse.GetState().LeftButton == ButtonState.Pressed) {
            //     _livingComponents.Reset();
            //     
            //     while(_livingComponents.Next()) {
            //         ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();
            //
            //         Vector2 currentMouseScreenPos = Mouse.GetState().Position.ToVector2();
            //         Vector2 currentMouseWorldPos = Owner.Core.RenderingUnit.ScreenLayer.LayerToCamera(currentMouseScreenPos);
            //
            //         currentMouseWorldPos = Owner.MainCameraTransform.Apply(currentMouseWorldPos);
            //
            //         _leftPos = currentMouseWorldPos;
            //         transform.Position = currentMouseWorldPos;
            //
            //         if(_livingComponents.HasCoComponent<Physics>()) {
            //             _leftVel = (currentMouseWorldPos - _previousMouseWorldPos) * 4;
            //             _livingComponents.GetCoComponent<Physics>().Velocity = _leftVel;
            //         }
            //         // Console.WriteLine(currentMouseWorldPos);
            //
            //         _previousMouseWorldPos = currentMouseWorldPos;
            //     }
            //
            //     _previouslyPressed = true;
            // } else if(_previouslyPressed) {
            //     Console.WriteLine($"Released at {_leftPos} with velocity {_leftVel}");
            //     _previouslyPressed = false;
            // }

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

        public override void SubscribeToEvents() {
            Subscribe((ref CollisionEvent collision) => {
                if(Owner.Components.HasComponent<Living>(collision.First) && Vector2.Dot(collision.Normal, Vector2.UnitY) > 0) {
                    Owner.Components.GetComponent<Living>(collision.First).Grounded = true;
                    _lastNormal = collision.Normal;
                }
            });
        }
    }
}