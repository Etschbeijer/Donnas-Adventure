using Godot;

public partial class MovingPlatform : AnimatableBody3D
{

    [Export]
    public Vector3 startPosition;
    [Export]
    public Vector3 targetPosition;

    [Export]
    public float travelTime = 2f;
    [Export]
    public float pauseTime = 0.7f;

    public Vector3 Velocity;
    Vector3 previousPosition;

    Tween movementTween;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        previousPosition = GlobalPosition;

        //Call important methods
        Movement();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        calculateVelocity((float)delta);
    }

    public void Movement()
    {
        StopMovement();

        // An infinite tween needs a nonzero duration or delay.
        if (travelTime <= 0f && pauseTime <= 0f)
            return;

        movementTween = CreateTween().SetLoops();
        movementTween.TweenProperty(this, "position", targetPosition, travelTime).SetTrans(Tween.TransitionType.Sine).SetDelay(pauseTime);
        movementTween.TweenProperty(this, "position", startPosition, travelTime).SetTrans(Tween.TransitionType.Sine).SetDelay(pauseTime);
    }

    public override void _ExitTree() => StopMovement();

    void StopMovement()
    {
        movementTween?.Kill();
        movementTween?.Dispose();
        movementTween = null;
    }

    private void calculateVelocity(float delta)
    {
        Vector3 currentPosition = GlobalPosition;
        Velocity = delta > 0f ? (currentPosition - previousPosition) / delta : Vector3.Zero;
        previousPosition = currentPosition;
    }
}
