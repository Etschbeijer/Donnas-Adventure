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
        targetPosition = new Vector3(Position.X, Position.Y - dropHeight, Position.Z);
    }

    public async Task Movement()
    {
        if (tree != null)
        {
            var travelTimer = GetTree().CreateTimer(travelTime);

            var moveTween = tree.CreateTween();

            moveTween.TweenProperty(this, "position", targetPosition, travelTime).SetTrans(Tween.TransitionType.Sine);
            await ToSignal(travelTimer, Timer.SignalName.Timeout);
        }
    }

    public async void OnPlatformEntered(CharacterBody3D body)
    {
        var collider = body as PlayerCharacter;
        if (collider != null)
        {
            PlayFallingSound();
            ControllExplosions(true);

            var timer = tree.CreateTimer(stayTime);
            await ToSignal(timer, Timer.SignalName.Timeout);
            await Movement();

            SetCollisionLayerValue(2, false);
            SetCollisionMaskValue(1, false);
            area.SetCollisionLayerValue(2, false);
            area.SetCollisionMaskValue(1, false);

            ControllExplosions(false);

            CallDeferred(nameof(Unload));
        }
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

    public void Unload()
    {
        QueueFree();
    }
}
