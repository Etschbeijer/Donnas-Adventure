using Godot;
using System;

public partial class StartZone : Area3D
{
    AudioStreamPlayer3D audioPlayer;

    Global global;

    RandomNumberGenerator rng = new RandomNumberGenerator();

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
    public void OnStartZoneExited(CharacterBody3D body)
    {
        var player = body as PlayerCharacter;

        if (player != null)
        {
            if (!global.CheckTime && audioPlayer.IsInsideTree())
            {
                audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
                audioPlayer.Play();

                global.StartTimer();
            }
        }
    }
}
