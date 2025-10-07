using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class WallJump : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void Enter(Dictionary msg = null)
    {
        Player.ControlFeetCollision(false, true);

        AnimationPlayer.Play("Wall_Jump", 0.1);

        Player.CanInput = false;
    }

    public override async void PhysicsUpdate(double delta)
    {
        CalculateGravityVelocity((float)delta);

        if (Player.CanInput)
        {
            Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);
        }

        if (Player.IsHurt)
        {
            Player.CanInput = true;
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            Player.ControlFeetCollision(true, false);
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            Player.CanInput = true;
            Player.TimeOnWall = 0f;
            AnimationPlayer.Play("Jump_Rise", 0.1);
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor() && IsAtTop((float)delta))
        {
            Player.CanInput = true;
            Player.ControlSaltoCollision(true);
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Player.TimeOnWall == 0f && Player.IsOnWallOnly())
        {
            Player.CanInput = true;
            StateMachine.TransitionTo(nameof(WallHanging));
        }

        if (Input.IsActionJustPressed("Jump") && Player.HasDoubleJump)
        {
            Player.CanInput = true;
            Player.HasDoubleJump = false;
            StateMachine.TransitionTo(nameof(AirRiseMovement));
        }

        if (Player.IsOnFloor())
        {
            if (Player.InputDir == Vector2.Zero)
            {
                Player.CanInput = true;
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControlFeetCollision(true, false);
                Player.ControlSaltoCollision(false);
                StateMachine.TransitionTo(nameof(Idle));
            }
            else
            {
                Player.CanInput = true;
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControlFeetCollision(true, false);
                Player.ControlSaltoCollision(false);
                StateMachine.TransitionTo(nameof(GroundMovement));
            }
        }

        if (Player.ShallBounce)
        {
            Player.CanInput = true;
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
