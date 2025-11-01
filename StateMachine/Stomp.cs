using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class Stomp : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    bool isGravityActive = true;

    public override void Enter(Dictionary msg = null)
    {
        Player.ControlFeetCollision(false, true);

        AnimationPlayer.Play("Jump_Land", 0.1);
        Player.ControlTrusters(false);
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Input.IsActionJustReleased("Stomp"))
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Input.IsActionJustPressed("Jump") && Player.HasDoubleJump)
        {
            Player.HasDoubleJump = false;
            Player.Velocity = Vector3.Zero;
            StateMachine.TransitionTo(nameof(AirRiseMovement));
        }

        if (isGravityActive)
        {
            CalculateGravityVelocity((float)delta);
        }

        if (Player.IsHurt)
        {
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (Player.IsOnFloor())
        {
            Player.HasDoubleJump = true;
            Player.FireExplosion(0.3f);
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
        if (Player.ShallRespawn())
        {
            Player.Respawn();
        }
        else if (Player.HasLost())
        {
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
        // Add the stomp to the gravity.
        Player.Gravity = Player.GetGravity();
        Player.PlayerVelocity += Player.Gravity * delta * 8;
        Player.PlayerVelocity = new Vector3(0, Player.PlayerVelocity.Y, 0);
        Player.TimeSinceLeftFloor += delta;
    }
}
