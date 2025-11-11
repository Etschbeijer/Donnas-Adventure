using Godot;
using System;

public partial class SpecialTreat : Area3D
{
    [Export]
    public TreatTypes TreatType;

    public Hud Hud;

    public bool isCollected = false;

    //Determines how many degrees the treat rotates each second.
    public const int RotSpeed = 60;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AnimationPlayer animationPlayer;
    Global global;

    AudioStreamPlayer3D audioPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        Hud = GetNode<Hud>("../../HUD");
        global = GetNode<Global>("/root/Global");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundSpecialTreat");
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
            SetCollisionLayerValue(3, false);
            SetCollisionMaskValue(1, false);

            animationPlayer.Play("bounce");

            Hud.UpdateLabelTreats();

            audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
            audioPlayer.Play();

            if (TreatType == TreatTypes.Cheese)
            {
                CollectedCheese();
            }

            if (TreatType == TreatTypes.Cookie)
            {
                CollectedCookie();
            }

            if (TreatType == TreatTypes.Meat)
            {
                CollectedMeat();
            }

            await ToSignal(audioPlayer, AudioStreamPlayer3D.SignalName.Finished);
            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
    }

    public void OnAnimationPlayerAnimationFinished(string animation)
    {
        if (animation == "bounce")
        {
            CallDeferred(nameof(Unload));
        }
    }

    public void Unload()
    {
        QueueFree();
    }

    public void CollectedCheese()
    {
        Hud.ShowCheese();
        global.AddSpecialTreat(TreatTypes.Cheese.ToString());
    }

    public void CollectedCookie()
    {
        Hud.ShowCookie();
        global.AddSpecialTreat(TreatTypes.Cookie.ToString());
    }

    public void CollectedMeat()
    {
        Hud.ShowMeat();
        global.AddSpecialTreat(TreatTypes.Meat.ToString());
    }
}
