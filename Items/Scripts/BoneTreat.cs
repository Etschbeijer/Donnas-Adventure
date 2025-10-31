using Godot;
using System;

public partial class BoneTreat : Area3D
{
    //Determines how many degrees the treat rotates each second.
    public const int RotSpeed = 80;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AnimationPlayer animationPlayer;
    Global global;

    AudioStreamPlayer3D audioPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        global = GetNode<Global>("/root/Global");
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundTreat");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        RotateY(Mathf.DegToRad(RotSpeed * (float)delta));
    }

    public async void OnBodyEntered(CharacterBody3D body)
    {
        var player = body as PlayerCharacter;

        if (player != null)
        {
            player.CanInput = false;
            player.PlayerVelocity = Vector3.Zero;
            player.IsInvulnerable = true;

            SetCollisionLayerValue(3, false);
            SetCollisionMaskValue(1, false);

            animationPlayer.Play("bounce");

            audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
            audioPlayer.Play();

            await ToSignal(audioPlayer, AudioStreamPlayer3D.SignalName.Finished);

            player.HasWon = true;

            CallDeferred(nameof(Unload));
        }
    }

    public void OnAnimationPlayerAnimationFinished(string animation)
    {
        //if (animation == "bounce")
        //{
        //    CallDeferred(nameof(Unload));
        //}
    }

    public void Unload()
    {
        QueueFree();
    }
}
