﻿using System;
using FoldEngine;
using FoldEngine.Commands;
using FoldEngine.Editor;
using FoldEngine.Events;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Registries;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace Sandbox;

public class WooferGameCore : IGameCore
{
    public WooferGameCore(GameRuntimeConfiguration runtimeConfig)
    {
        Console.WriteLine("Constructing Core");
        FoldGame = new FoldGame(this, runtimeConfig);

        RegistryUnit = new RegistryUnit(this);
        RenderingUnit = new WooferRenderingUnit(this);
        InputUnit = new InputUnit(this);
        AudioUnit = new AudioUnit(this);
        CommandQueue = new CommandQueue(this);
        Resources = new ResourceCollections(this);
        ResourceIndex = new ResourceIndex(this);
        Events = new EventMap();
    }

    public float TimeScale => 1;
    public FoldGame FoldGame { get; }

    public RegistryUnit RegistryUnit { get; }

    public IRenderingUnit RenderingUnit { get; }

    public Scene ActiveScene { get; set; }

    public InputUnit InputUnit { get; }

    public AudioUnit AudioUnit { get; }

    public CommandQueue CommandQueue { get; }

    public ResourceCollections Resources { get; }

    public ResourceIndex ResourceIndex { get; }
    public EventMap Events { get; }

    public void Initialize()
    {
        Console.WriteLine("Initializing Core");
        RegistryUnit.Initialize();
        RenderingUnit.Initialize();
        AudioUnit.Initialize();
        ResourceIndex.Update();

        ActiveScene = new EditorScene(this);
        ActiveScene.Systems.Get<EditorBase>().OpenScene(new DemoScene(this));
        ActiveScene.Systems.Get<EditorBase>().OpenScene(new Scene(this, "blank"));
    }

    public void LoadContent()
    {
        Console.WriteLine("Loading Core Content");

        foreach (string inputName in ResourceIndex.GetIdentifiersInGroup<InputDefinition>("#default"))
        {
            var identifier = new ResourceIdentifier(inputName);
            Resources.Load<InputDefinition>(ref identifier, d => { InputUnit.Setup((InputDefinition)d); });
        }
    }

    public void Input()
    {
        ActiveScene?.Input();
        Events.FlushAfterSystem();
        Events.FlushEnd();
    }

    public void Update()
    {
        ActiveScene?.Update();
        Events.FlushAfterSystem();
        Events.FlushEnd();
    }

    public void Render()
    {
        ActiveScene?.Render(RenderingUnit);
        Events.FlushAfterSystem();
        Events.FlushEnd();
    }
}