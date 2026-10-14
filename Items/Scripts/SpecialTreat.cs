using Godot;

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

    public void OnBodyEntered(CharacterBody3D body)
    {
        if (body is not PlayerCharacter || isCollected)
            return;

        isCollected = true;
        SetCollisionLayerValue(3, false);
        SetCollisionMaskValue(1, false);

        animationPlayer.Play("bounce");
        Hud.UpdateLabelTreats();
        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();
        CollectSpecialTreat(TreatType);
        // The scene's animation_finished connection already handles cleanup.
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

    public void CollectedCheese() => CollectSpecialTreat(TreatTypes.Cheese);

    public void CollectedCookie() => CollectSpecialTreat(TreatTypes.Cookie);

    public void CollectedMeat() => CollectSpecialTreat(TreatTypes.Meat);

    void CollectSpecialTreat(TreatTypes type)
    {
        switch (type)
        {
            case TreatTypes.Cheese: Hud.ShowCheese(); break;
            case TreatTypes.Cookie: Hud.ShowCookie(); break;
            case TreatTypes.Meat: Hud.ShowMeat(); break;
            default: return;
        }

        global.AddSpecialTreat(type.ToString());
    }
}
