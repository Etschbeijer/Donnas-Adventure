using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class WallJump : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    private bool startJumping { get; set; } = false;

    public override void Enter(Dictionary msg = null)
    {
        Player.ControlFeetCollision(false, true);

        if (msg is not null && msg.Keys.Contains("doJump") && Player.HasDoubleJump)
        {
            AnimationPlayer.Play("Wall_Jump", 0.1);

            startJumping = true;
        }
    }

    public override async void PhysicsUpdate(double delta)
    {
        CalculateGravityVelocity((float)delta);

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        if (Player.IsHurt)
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            Player.ControlFeetCollision(true, false);
            FaceAwayFromWall();
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            Player.TimeonWall = 0f;
            FaceAwayFromWall();
            AnimationPlayer.Play("Jump_Rise", 0.1);
            startJumping = false;
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor() && IsAtTop((float)delta))
        {
            Player.ControlSaltoCollision(true);
            FaceAwayFromWall();
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Player.TimeonWall == 0f && Player.IsOnWallOnly())
        {
            FaceAwayFromWall();
            StateMachine.TransitionTo(nameof(WallHanging));
        }

        if (Input.IsActionJustPressed("Jump") && Player.HasDoubleJump)
        {
            Player.HasDoubleJump = false;
            FaceAwayFromWall();
            StateMachine.TransitionTo(nameof(AirRiseMovement));
        }

        if (Player.IsOnFloor() && !startJumping)
        {
            if (Player.InputDir == Vector2.Zero)
            {
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControlFeetCollision(true, false);
                Player.ControlSaltoCollision(false);
                StateMachine.TransitionTo(nameof(Idle));
            }
            else
            {
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControlFeetCollision(true, false);
                Player.ControlSaltoCollision(false);
                StateMachine.TransitionTo(nameof(GroundMovement));
            }
        }

        if (Player.ShallBounce)
        {
            StateMachine.TransitionTo(nameof(Bounce));
        }
    }

    public override async void Update(double delta)
    {
        if (Player.CurrentHealth <= 0)
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            StateMachine.TransitionTo(nameof(Lose));
        }

        if (Player.HasWon)
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            StateMachine.TransitionTo(nameof(Win));
        }
    }

    public void CalculateGravityVelocity(float delta)
    {
        // Add the gravity.
        if (Player.IsOnFloor())
        {
            Player.TimeSinceLeftFloor = 0;
        }
        else
        {
            Player.Gravity = Player.GetGravity();
            Player.PlayerVelocity += Player.Gravity * delta;
            Player.TimeSinceLeftFloor += delta;
        }
    }

    public void FaceAwayFromWall()
    {
        Player.Rotation = new Vector3(Player.Rotation.X, -Player.Rotation.Y, Player.Rotation.Z);
    }

    public bool IsAtTop(float delta)
    {
        return (Player.PlayerVelocity.Y <= (Player.Gravity * delta).Y);
    }
}
