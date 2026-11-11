using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class FallingPlatform : AnimatableBody3D
{
    [Export]
    public float stayTime = 1f;

    [Export]
    public float dropHeight = 2f;

    float travelTime = 0.5f;

    Vector3 targetPosition;

    Area3D area;

    SceneTree tree;

    AudioStreamPlayer3D audioPlayer;

    Node3D [] explosions;
    GpuParticles3D [] particleExplosions = [];

    RandomNumberGenerator rng = new RandomNumberGenerator();

    Vector3 spawnPosition;
    uint bodyLayer;
    uint bodyMask;
    uint areaLayer;
    uint areaMask;

    // Bumped on every respawn so a drop that is still awaiting a timer stops itself.
    int cycle;
    bool isTriggered;

    Tween dropTween;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        area = GetNode<Area3D>("Area3D");
        tree = GetTree();
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundEarthBlock");
        explosions = GetNode<Node3D>("Explosions").GetChildren().OfType<Node3D>().Where(explosion => explosion.Name.ToString().Contains("EarthExplosion")).ToArray();
        particleExplosions = 
            explosions.SelectMany(explosion => explosion
                .GetChildren()
                .OfType<GpuParticles3D>()
                .Where(explosion => explosion.Name.ToString().Contains("GPUParticles3D"))
            )
            .ToArray();
        

        //Set initial values
        AddToGroup("ResetOnRespawn");
        spawnPosition = Position;
        bodyLayer = CollisionLayer;
        bodyMask = CollisionMask;
        areaLayer = area.CollisionLayer;
        areaMask = area.CollisionMask;
        targetPosition = new Vector3(spawnPosition.X, spawnPosition.Y - dropHeight, spawnPosition.Z);
    }

    public async Task Movement()
    {
        if (tree != null)
        {
            var travelTimer = GetTree().CreateTimer(travelTime);

            dropTween = tree.CreateTween();

            dropTween.TweenProperty(this, "position", targetPosition, travelTime).SetTrans(Tween.TransitionType.Sine);
            await ToSignal(travelTimer, Timer.SignalName.Timeout);
        }
    }

    public async void OnPlatformEntered(CharacterBody3D body)
    {
        if (body is not PlayerCharacter || isTriggered)
        {
            return;
        }

        isTriggered = true;
        int myCycle = cycle;

        PlayFallingSound();
        ControllExplosions(true);

        var timer = tree.CreateTimer(stayTime);
        await ToSignal(timer, Timer.SignalName.Timeout);
        if (cycle != myCycle || !IsInstanceValid(this))
        {
            return;
        }

        await Movement();
        if (cycle != myCycle || !IsInstanceValid(this))
        {
            return;
        }

        SetCollisionLayerValue(2, false);
        SetCollisionMaskValue(1, false);
        area.SetCollisionLayerValue(2, false);
        area.SetCollisionMaskValue(1, false);

        ControllExplosions(false);

        Hide();
    }

    public void ResetOnRespawn()
    {
        cycle++;
        dropTween?.Kill();
        dropTween = null;
        Position = spawnPosition;
        CollisionLayer = bodyLayer;
        CollisionMask = bodyMask;
        area.CollisionLayer = areaLayer;
        area.CollisionMask = areaMask;
        ControllExplosions(false);
        Show();
        isTriggered = false;
    }

    public void ControllExplosions(bool onOff)
    {
        foreach (GpuParticles3D explision in particleExplosions)
        {
            explision.Emitting = onOff;
        }
    }

    public void PlayFallingSound()
    {
        audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
        audioPlayer.Play();
    }
}
