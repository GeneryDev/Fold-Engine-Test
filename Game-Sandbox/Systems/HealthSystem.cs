using System;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sandbox.Components;

namespace Sandbox.Systems;

[GameSystem("sandbox:health", ProcessingCycles.Update | ProcessingCycles.Render)]
public class HealthSystem : GameSystem
{
    private ComponentIterator<Living> _livingComponents;
    private Vector2 _previousMousePos;

    private Vector2 _rightPressedScreenPos;
    private Vector2 _rightPressedWorldPos;
    private bool _rightPreviouslyPressed;

    public override void Initialize()
    {
        _livingComponents = CreateComponentIterator<Living>(IterationFlags.None);
    }

    public override void OnUpdate()
    {
        //Console.WriteLine("HealthSystem update");
        //Owner.Components.DebugPrint<Transform>();
        //Owner.Components.DebugPrint<Living>();

        _livingComponents.Reset();

        while (_livingComponents.Next())
        {
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

    public override void OnRender(IRenderingUnit renderer)
    {
        _livingComponents.Reset();

        var currentMouseScreenPos = Mouse.GetState().Position.ToVector2();
        Vector2 currentMouseWorldPos = renderer.WindowLayer.LayerToCamera(currentMouseScreenPos);

        bool rightPressed = Mouse.GetState().RightButton == ButtonState.Pressed;

        if (rightPressed && !_rightPreviouslyPressed)
        {
            _rightPressedScreenPos = currentMouseScreenPos;
            _rightPressedWorldPos = currentMouseWorldPos;
        }

        while (_livingComponents.Next())
        {
            ref Transform transform = ref _livingComponents.GetCoComponent<Transform>();

            if (_livingComponents.HasCoComponent<Physics>())
            {
                ref Physics physics = ref _livingComponents.GetCoComponent<Physics>();
                if (physics.Static) continue;

                if (rightPressed)
                {
                    physics.Torque = default;
                    physics.AccelerationFromForce = default;
                    physics.Velocity = default;
                    physics.AngularVelocity = default;

                    renderer.MainGroup["gizmos"]
                        .Surface.GizBatch.DrawLine(currentMouseScreenPos, _rightPressedScreenPos, Color.Lime);
                }

                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    transform.Position = currentMouseWorldPos;
                    physics.Velocity = (currentMouseWorldPos - _previousMousePos) * 8;
                    physics.AngularVelocity = default;
                }

                if (!rightPressed && _rightPreviouslyPressed)
                {
                    physics.ApplyForce((currentMouseWorldPos - _rightPressedWorldPos) * 100,
                        currentMouseWorldPos - transform.Position, ForceMode.Instant);
                    Console.WriteLine($"Torque: {physics.Torque}");
                    Console.WriteLine($"Linear Acceleration: {physics.AccelerationFromForce}");
                }
            }
        }

        _previousMousePos = currentMouseWorldPos;
        _rightPreviouslyPressed = rightPressed;
    }
}