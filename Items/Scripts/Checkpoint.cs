using Godot;
using System;
using System.Reflection.Emit;

public partial class Checkpoint : AnimatableBody3D
{
    [Export]
    public Vector3 PlayerSpawnPosition;
    [Export]
    public Vector3 PlayerSpawnRotation;

    [Export]
    public bool IsActive = false;

    public AnimationPlayer AnimationPlayer { get; set; }

    private float currentTime = 0f;
    private float totalTime = 0f;

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
    }

    public void ChangeMaterial(int layer, Material material)
    {
        meshInstance3D.SetSurfaceOverrideMaterial(layer, material);
    }

    public async void OnCheckpointEntered(CharacterBody3D body)
	{
        var sceneTree = GetTree();
        var player = body as PlayerCharacter;

        if (player != null)
        {
            PlaySoundCheckPointEntered();
            player.TopSpeedTimer += 2f;
            player.UpdateSpeedGauge();

            if (!IsActive)
            {
                IsActive = true;

                var prupleMaterial = GD.Load<Material>("res://Materials/donna_flame.tres");
                var blackOutlineMaterial = GD.Load<Material>("res://Materials/material_black_outline.tres");

                prupleMaterial.NextPass = blackOutlineMaterial;

                ChangeMaterial(0, prupleMaterial);

                player.SetSpawnParameters(PlayerSpawnPosition, new Vector3(normalizeDegrees(PlayerSpawnRotation.X), normalizeDegrees(PlayerSpawnRotation.Y), normalizeDegrees(PlayerSpawnRotation.Z)));
            }

            AnimationPlayer.SpeedScale = 4f;
            var timer = sceneTree.CreateTimer(1f);
            await ToSignal(timer, Timer.SignalName.Timeout);
            AnimationPlayer.SpeedScale = 1f;
        }
    }

    private float normalizeDegrees(float degree)
    {
        var angleRad = Mathf.DegToRad(degree);
        return Mathf.Wrap(angleRad, -Mathf.Pi, Mathf.Pi);
    }

    public void PlaySoundCheckPointEntered()
    {
        playSoundCheckPointEntered.PitchScale = rng.RandfRange(0.9f, 1.1f);
        playSoundCheckPointEntered.Play();
    }
}
