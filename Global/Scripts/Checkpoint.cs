using Godot;
using System;
using System.Reflection.Emit;

public partial class Checkpoint : AnimatableBody3D
{
    public AnimationPlayer AnimationPlayer { get; set; }

    bool isActive = false;

    private float currentTime = 0f;
    private float totalTime = 0f;

    Vector3 playerSpawn;

    MeshInstance3D meshInstance3D;

    AudioStreamPlayer3D playSoundCheckPointEntered;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        meshInstance3D = GetNode<MeshInstance3D>("MeshInstance3D");
        playSoundCheckPointEntered = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        //Set initial values
        playerSpawn = new Vector3(Position.X, Position.Y, Position.Z + 1);
    }

    public void ChangeMaterial(int layer, Material material)
    {
        meshInstance3D.SetSurfaceOverrideMaterial(layer, material);
    }

    public async void OnCheckpointEntered(CharacterBody3D body)
	{
        var sceneTree = GetTree();
        var player = body as PlayerCharacter;

        if (player != null && !isActive)
        {
            PlaySoundCheckPointEntered();

            isActive = true;

            var prupleMaterial = GD.Load<Material>("res://Materials/donna_flame.tres");
            var blackOutlineMaterial = GD.Load<Material>("res://Materials/material_black_outline.tres");

            prupleMaterial.NextPass = blackOutlineMaterial;

            ChangeMaterial(0, prupleMaterial);

            player.SetSpawnPoint(playerSpawn);
        }

        AnimationPlayer.SpeedScale = 4f;

        var timer = sceneTree.CreateTimer(1f);
        await ToSignal(timer, Timer.SignalName.Timeout);
        AnimationPlayer.SpeedScale = 1f;
    }

    public void PlaySoundCheckPointEntered()
    {
        playSoundCheckPointEntered.PitchScale = rng.RandfRange(0.9f, 1.1f);
        playSoundCheckPointEntered.Play();
    }
}
