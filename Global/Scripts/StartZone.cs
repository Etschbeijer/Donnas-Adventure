using Godot;
using System;

public partial class StartZone : Area3D
{
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public AudioStreamPlayer3D audioPlayer;

    Global global;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundStart");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void onStartZoneEntered(CharacterBody3D body)
    {
        if (body is PlayerCharacterMovement player)
        {
            audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
            audioPlayer.Play();

            global.startTimer();
            SetCollisionMaskValue(1, false);
        }
    }

    
}
