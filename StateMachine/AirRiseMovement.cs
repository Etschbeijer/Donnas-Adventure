using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class AirRiseMovement : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    private bool startJumping { get; set; } = false;

    public override void Enter(Dictionary msg = null)
    {
        Player.ControllFeetCollision(false, true);

        if (msg is not null && msg.Keys.Contains("doJump") && Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Start", 0.1, 2);

            Player.PlayerVelocity.Y = Player.JumpVelocity;

            startJumping = true;
        }
        else
        {
            AnimationPlayer.Play("Jump_Double", 0.1);
            Player.ControlTrusters(true);

            Player.PlayerVelocity.Y = Player.JumpVelocity;

            startJumping = false;
        }
    }

    public override async void PhysicsUpdate(double delta)
    {
        if (Input.IsActionJustPressed("Jump") && !Player.IsOnFloor() && Player.HasDoubleJump)
        {
            DoubleJump();
        }

        CalculateGravityVelocity((float) delta);

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        if (Player.IsHurt)
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
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
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControllFeetCollision(true, false);
                Player.ControllSaltoCollision(false);
                StateMachine.TransitionTo(nameof(Idle));
            }
            else
            {
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControllFeetCollision(true, false);
                Player.ControllSaltoCollision(false);
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
        Player.PlayerVelocity.Y = 0f;
        Player.Velocity = Player.PlayerVelocity;
        StateMachine.TransitionTo(nameof(GetHit));
    }
}
