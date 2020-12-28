using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using Sandbox.Components;
using System;
using System.Collections.Generic;
using System.Text;
using FoldEngine;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Sandbox.Systems {
    [GameSystem("sandbox:health", ProcessingCycles.Update | ProcessingCycles.Render)]
    public class HealthSystem : GameSystem {
        private ComponentIterator<Living> _livingComponents;

        internal override void Initialize() {
            _livingComponents = CreateComponentIterator<Living>(IterationFlags.None);
        }

        public override void OnUpdate() {
            //Console.WriteLine("HealthSystem update");
            //Owner.Components.DebugPrint<Transform>();
            //Owner.Components.DebugPrint<Living>();

            _livingComponents.Reset();

            while(_livingComponents.Next()) {
                //Console.WriteLine($"Entity {LivingComponents.GetEntityId()} has:");

                ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

                // if(transform.Parent.IsNotNull) {
                //     transform.LocalPosition += new Vector2(Time.DeltaTime, 0);
                // } else {
                //     transform.LocalRotation += Time.DeltaTime;
                // }

                // if(transform.Parent.IsNotNull) {
                //     // transform.LocalPosition.X += Time.DeltaTime * 0.5f;
                //     // transform.LocalRotation += Time.DeltaTime;
                //     if(_livingComponents.HasCoComponent<MeshRenderable>()) {
                //         ref MeshRenderable meshRenderable = ref _livingComponents.GetCoComponent<MeshRenderable>();
                //         meshRenderable.UVOffset.Y = ((int) (Time.TotalTime * 16)) % 11;
                //     }
                // } else {
                //     // transform.LocalScale.X += Time.DeltaTime * 0.5f;
                // }
            }

            /*
            foreach(var living in LivingComponents)
            {
                Console.WriteLine($"Entity {living.EntityId} has:");
                //Console.WriteLine(living.Get<Transform>());
                //Console.WriteLine(living.Get<Living>());
            }*/
            //Console.WriteLine();
        }

        private Vector2 _previousMousePos;

        public override void OnRender(IRenderingUnit renderer) {
            _livingComponents.Reset();
            
            Vector2 currentMousePos = RenderingLayer.ScreenToWorld(renderer.Layers["screen"],
                Mouse.GetState().Position.ToVector2());

            while(_livingComponents.Next()) {
                ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

                if(_livingComponents.HasCoComponent<Physics>() && Mouse.GetState().LeftButton == ButtonState.Pressed) {
                    ref Physics physics = ref _livingComponents.GetCoComponent<Physics>();
                    
                    transform.Position = RenderingLayer.ScreenToWorld(renderer.Layers["screen"],
                        Mouse.GetState().Position.ToVector2());
                    
                    physics.Velocity = (currentMousePos - _previousMousePos) * 8;
                }
            }
            
            _previousMousePos = currentMousePos;
        }
    }
}