using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class AirRiseMovement : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    private bool startJumping { get; set; } = false;

    private float jumpHoldTimer = 0f;

    public override void Enter(Dictionary msg = null)
    {
        Player.TimeOnWall = 0f;
        Player.ControlFeetCollision(false, true);

        jumpHoldTimer = 0f;

        if (msg is not null && msg.Keys.Contains("doJump") && Player.HasDoubleJump)
        {
            AnimationPlayer.Play("Jump_Start", 0.1, 2);

            Player.PlayerVelocity.Y = Player.JumpVelocity * 0.5f;

            startJumping = true;
        }
        else
        {
            AnimationPlayer.Play("Jump_Double", 0.1);
            Player.ControlTrusters(true);

            Player.PlayerVelocity.Y = Player.JumpVelocity * 0.5f;

            startJumping = false;
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Input.IsActionPressed("Jump") && jumpHoldTimer < Player.JumpTime)
        {
            jumpHoldTimer += (float)delta;
            var weight = (float)delta / Player.JumpTime;
            CalculateMaximumJumpHeigh(weight);
        }

        if (Input.IsActionJustPressed("Jump") && Player.HasDoubleJump)
        {
            jumpHoldTimer = 0f;
            DoubleJump();
        }

        CalculateGravityVelocity((float) delta);

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        if (Player.PlatformVelocity != Vector3.Zero)
        {
            var adaptedVelocity = new Vector3(Player.PlatformVelocity.X, Player.PlatformVelocity.Y, Player.PlatformVelocity.Z);
            Player.PlayerVelocity += (adaptedVelocity * 0.5f);
            Player.PlatformVelocity = Vector3.Zero;
        }

        if (Input.IsActionJustPressed("Stomp") || (Input.IsActionPressed("Stomp")))
        {
            Player.PlayerVelocity.Y = 0;
            StateMachine.TransitionTo(nameof(Stomp));
        }

        if (Player.IsHurt)
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            Player.ControlFeetCollision(true, false);
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Rise", 0.1);
            startJumping = false;
        }

        if(!Player.IsOnFloor() && IsAtTop((float) delta))
        {
            Player.ControlSaltoCollision(true);
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Player.TimeOnWall == 0 && Player.IsOnWallOnly())
        {
            StateMachine.TransitionTo(nameof(WallHanging));
        }

        if (Player.IsOnFloor() && !startJumping)
        {
            Player.HasDoubleJump = true;
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

    public override void Update(double delta)
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

    public void CalculateMaximumJumpHeigh(float weight)
    {
        var newJumpHeight = Mathf.Lerp(Player.PlayerVelocity.Y, Player.JumpVelocity, weight);
        Player.PlayerVelocity = new Vector3(Player.PlayerVelocity.X, (float) newJumpHeight, Player.PlayerVelocity.Z);
    }

    public void DoubleJump()
    {
        Player.HasDoubleJump = false;
        Player.PlayerVelocity.Y = Player.JumpVelocity * 0.5f;
        AnimationPlayer.Play("Jump_Double", 0.1);
        Player.ControlTrusters(true);
    }

    public bool IsAtTop(float delta)
    {
        return (Player.PlayerVelocity.Y <= (Player.Gravity * delta).Y);
    }
}
