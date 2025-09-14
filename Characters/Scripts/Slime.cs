using Godot;
using System;
using System.Threading.Tasks;
using static Godot.TextServer;

public partial class Slime : CharacterBody3D
{
    public float movementSpeed = 2.0f;

    [Export]
    public double turnTime = 0.6;

    [Export]
	Vector3 direction = new Vector3(0, 0, 1).Normalized();

    bool isTurning = false;

    Vector3 rotation;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AnimationPlayer animationPlayer;
    Global global;

    Area3D hurtbox;
    Area3D hitbox;

    RayCast3D floorDetector;

    AudioStreamPlayer3D audioPlayer;

    SoundManager soundManager;

    public override void _Ready()
	{
        //Get nodes
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        global = GetNode<Global>("/root/Global");
        hurtbox = GetNode<Area3D>("Hurtbox");
        hitbox = GetNode<Area3D>("Hitbox");
        floorDetector = GetNode<RayCast3D>("RayCast3D");
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundSlime");
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        rotation = Rotation;
    }

    public override async void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

        velocity.Z = movementSpeed * direction.Z;
        velocity.X = movementSpeed * direction.X;

        Velocity = velocity;

		MoveAndSlide();

		if ((IsOnWall() || !floorDetector.IsColliding()) && !isTurning && IsOnFloor())
		{
			await turnAround();
        }
	}

	public async Task turnAround()
	{
        isTurning = true;

        var dir = direction;
        direction = Vector3.Zero;

        var timer = GetTree().CreateTimer(turnTime);

        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();

        var turnTween = CreateTween();

		turnTween.TweenProperty(this, "rotation_degrees", new Vector3(Rotation.X, 180, Rotation.Z), turnTime).AsRelative();

        await ToSignal(timer, Timer.SignalName.Timeout);

        direction = -dir;

		isTurning = false;
    }

    public async void Hit(Area3D hurtBox)
    {
        var body = hurtBox.GetOwner();

        if (body is PlayerCharacterMovement player)
        {
            var hitFromAbove = !player.IsOnFloor() || player.timeSinceLeftFloor > player.coyoteTime;
            if (hitFromAbove)
            {
                await death(body as PlayerCharacterMovement);
            }
        }
    }

    public void onHitboxEntered(Area3D hurtbox)
    {
        //var body = hurtbox.GetOwner();
        //if (body is PlayerCharacterMovement)
        //{
        //    GD.Print("Slime onHitboxEntered");
        //    GD.Print($"Donnas hurtbox: {hurtbox.Name}");
        //}
    }

    public async void onHurtboxEntered(Area3D hitbox)
    {
        var body = hitbox.GetOwner();
        if (body is PlayerCharacterMovement)
        {
            await death(body as PlayerCharacterMovement);
        }
        else if(body is Slime slime && !isTurning)
        {
            slime.turnAround();
            turnAround();
        }
    }

    public async Task death(PlayerCharacterMovement body)
    {
        animationPlayer.Play("Squash", 0.1);

        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();

        direction = Vector3.Zero;
        movementSpeed = 0;

        body.bounce();

        hurtbox.SetCollisionLayerValue(8, false);
        hurtbox.SetCollisionMaskValue(7, false);

        hitbox.SetCollisionLayerValue(9, false);
        hitbox.SetCollisionMaskValue(6, false);

        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        CallDeferred(nameof(unload));
    }

    public void unload()
    {
        QueueFree();
    }

    public void loadDefeatScene()
	{
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_defeat.tscn");
    }
}
