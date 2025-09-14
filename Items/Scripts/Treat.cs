using Godot;
using System;
using System.Threading.Tasks;

public partial class Treat : Area3D
{
    //Determines how many degrees the treat rotates each second.
    public const int RotSpeed = 60;

    [Export]
    public Hud hud;

    public bool isCollected = false;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AnimationPlayer animationPlayer;
    Global global;

    AudioStreamPlayer3D audioPlayer;

    SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        global = GetNode<Global>("/root/Global");
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundTreat");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        RotateY(Mathf.DegToRad(RotSpeed * (float)delta));
    }

    public async void onBodyEntered(CharacterBody3D body)
    {
        global.increaseTreats();
        SetCollisionLayerValue(3, false);
        SetCollisionMaskValue(1, false);

        animationPlayer.Play("bounce");

        hud.updateLabelTreats();

        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();

        await ToSignal(audioPlayer, AudioStreamPlayer2D.SignalName.Finished);

        if (global.collectedTreats == global.numberOfTreats && body is PlayerCharacterMovement)
        {
            var player = body as PlayerCharacterMovement;

            player.CallDeferred(nameof(player.controllHurtBoxes), false);
            player.hasWon = true;

            global.stopTimer();

            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

            await player.playWinAnimation();
            await player.gameOver("res://Menu/Nodes/menu_win.tscn");
        }
        else
        {
            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
    }

    public void onAnimationPlayerAnimationFinished(string animation)
    {
        if (animation == "bounce")
        {
            CallDeferred(nameof(unload));
        }
    }

    public void unload()
    {
        QueueFree();
    }

    public void loadWinScene()
    {
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_win.tscn");
    }
}
