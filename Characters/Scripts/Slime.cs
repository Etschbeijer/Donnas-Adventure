using Godot;
using System;
using System.Threading.Tasks;
using static Godot.TextServer;

public partial class Slime : CharacterBody3D
{
    public float MovementSpeed = 2.0f;

    [Export]
    public double TurnTime = 0.6;

    [Export]
	Vector3 direction = new Vector3(0, 0, 1).Normalized();

    bool isTurning = false;

    Vector3 rotation;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AnimationPlayer animationPlayer;
    Global global;

    GpuParticles3D slimeBurst;

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
        slimeBurst = GetNode<Node3D>("SlimeBurst").GetNode<GpuParticles3D>("GPUParticles3D");

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

        velocity.Z = MovementSpeed * direction.Z;
        velocity.X = MovementSpeed * direction.X;

        Velocity = velocity;

		MoveAndSlide();

		if ((IsOnWall() || !floorDetector.IsColliding()) && !isTurning && IsOnFloor())
		{
			await TurnAround();
        }
	}

	public async Task TurnAround()
	{
        isTurning = true;

        var dir = direction;
        direction = Vector3.Zero;

        var timer = GetTree().CreateTimer(TurnTime);

        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();

        var turnTween = CreateTween();

		turnTween.TweenProperty(this, "rotation_degrees", new Vector3(Rotation.X, 180, Rotation.Z), TurnTime).AsRelative();

        await ToSignal(timer, Timer.SignalName.Timeout);

        direction = -dir;

		isTurning = false;
    }

    public async void Hit(Area3D hurtBox)
    {
        var player = hurtBox.GetOwner() as PlayerCharacter;

        if (player != null)
        {
            var hitFromAbove = !player.IsOnFloor() || player.TimeSinceLeftFloor > player.CoyoteTime;
            if (hitFromAbove)
            {
                await Death(player);
            }
        }
    }

    public async void OnHurtboxEntered(Area3D hitbox)
    {
        var body = hitbox.GetOwner();
        var player = body as PlayerCharacter;

        if (player != null)
        {
            await Death(player);
        }
        else if(body is Slime slime && !isTurning)
        {
            slime.TurnAround();
            TurnAround();
        }
    }

    public async Task Death(PlayerCharacter player)
    {
        animationPlayer.Play("Squash", 0.1);

        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();
        MovementSpeed = 0;
        slimeBurst.Emitting = true;
        direction = Vector3.Zero;

        player.ShallBounce = true;

        hurtbox.SetCollisionLayerValue(8, false);
        hurtbox.SetCollisionMaskValue(7, false);

        hitbox.SetCollisionLayerValue(9, false);
        hitbox.SetCollisionMaskValue(6, false);

        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        CallDeferred(nameof(Unload));
    }

    public void Unload()
    {
        QueueFree();
    }

    public void LoadDefeatScene()
	{
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_defeat.tscn");
    }
}
