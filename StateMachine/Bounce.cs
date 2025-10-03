using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Bounce : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    private bool startJumping { get; set; } = false;

    public override void Enter(Dictionary msg = null)
    {
        Player.HasDoubleJump = true;
        Player.ShallBounce = false;

        Player.TimeSinceLeftFloor = 0;

        AnimationPlayer.Play("Jump_Start", 0.1, 2);

        Player.PlayerVelocity.Y = Player.JumpVelocity * 0.7f;

        startJumping = true;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Input.IsActionJustPressed("Jump") && !Player.IsOnFloor() && Player.HasDoubleJump)
        {
            DoubleJump();
        }

        CalculateGravityVelocity((float) delta);

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        if (Player.IsHurt)
        {
            Player.ControllFeetCollision(true, false);
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Rise", 0.1);
            startJumping = false;
        }

        //if (Input.IsActionJustPressed("Attack"))
        //{
        //    StateMachine.TransitionTo("Attack1");
        //}

        if(!Player.IsOnFloor() && IsAtTop((float) delta))
        {
            Player.ControllSaltoCollision(true);
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Player.IsOnFloor() && !startJumping)
        {
            Player.HasDoubleJump = true;
            if (Player.InputDir == Vector2.Zero)
            {
                Player.ControllFeetCollision(true, false);
                Player.ControllSaltoCollision(false);
                StateMachine.TransitionTo(nameof(Idle));
            }
            else
            {
                Player.ControllFeetCollision(true, false);
                Player.ControllSaltoCollision(false);
                StateMachine.TransitionTo(nameof(GroundMovement));
            }
        }
    }

    public override void Update(double delta)
    {
        if (Player.CurrentHealth <= 0)
        {
            StateMachine.TransitionTo(nameof(Lose));
        }

        if (Player.HasWon)
        {
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

    public void DoubleJump()
    {
        Player.HasDoubleJump = false;
        Player.PlayerVelocity.Y = Player.JumpVelocity;
        AnimationPlayer.Play("Jump_Double", 0.1);
        Player.ControlTrusters(true);
    }

    public bool IsAtTop(float delta)
    {
        return (Player.PlayerVelocity.Y <= (Player.Gravity * delta).Y);
    }

    public void OnHit()
    {
        StateMachine.TransitionTo(nameof(GetHit));
    }
}
