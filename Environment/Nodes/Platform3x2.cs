using Godot;
using System;
using System.Threading.Tasks;

public partial class Platform3x2 : AnimatableBody3D
{

    [Export]
    public Vector3 startPosition;
    [Export]
    public Vector3 targetPosition;

    [Export]
    public float travelTime = 2f;
    [Export]
    public float pauseTime = 0.7f;

    SceneTree tree;

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
	{
        tree = GetTree();
        await movement();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public async Task movement()
    {
        if (tree != null)
        {
            var travelTimer = GetTree().CreateTimer(2 * (travelTime + pauseTime));

            var moveTween = CreateTween();

            moveTween.TweenProperty(this, "position", targetPosition, travelTime).SetTrans(Tween.TransitionType.Sine).SetDelay(pauseTime);
            moveTween.TweenProperty(this, "position", startPosition, travelTime).SetTrans(Tween.TransitionType.Sine).SetDelay(pauseTime);
            await ToSignal(travelTimer, Timer.SignalName.Timeout);
            await movement();
        }
    }
}
