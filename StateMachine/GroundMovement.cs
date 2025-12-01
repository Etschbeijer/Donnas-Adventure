using Godot;
using Godot.Collections;
using System;
using static Godot.TextServer;

public partial class GroundMovement : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void Enter(Dictionary msg = null)
    {
        Player.TimeOnWall = 0f;
        Player.TimeSinceLeftFloor = 0;

        PlayMovementAnimation();
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        PlayMovementAnimation();

        if (!Player.IsOnFloor())
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));

            return;
        }

        if (Player.IsHurt)
        {
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (Input.IsActionJustPressed("Jump"))
        {
            Dictionary msg = new Dictionary();
            msg["doJump"] = true;

            StateMachine.TransitionTo(nameof(AirRiseMovement), msg);
        }
        //else if (Input.IsActionJustPressed("Attack"))
        //{
        //    StateMachine.TransitionTo("Attack1");
        //}
        else if (Player.InputDir == Vector2.Zero && Player.IsOnFloor())
        {
            StateMachine.TransitionTo(nameof(Idle));
        }
    }

    public override void Update(double delta)
    {
        if (Player.ShallRespawn() || Player.HasLost())
        {
            StateMachine.TransitionTo(nameof(Despawn));
        }

        if (Player.HasWon)
        {
            StateMachine.TransitionTo(nameof(Win));
        }
    }

    public void PlayMovementAnimation()
    {
        //Play animation
        if (Player.MovementSpeed > 6)
        {
            PlaySkateAnimation();
        }
        else
        {
            PlayWalkAnimation();
        }
    }

    public void PlaySkateAnimation()
    {
        Player.ControlTrusters(true);

        if (Player.MovementSpeed > 8)
        {
            AnimationPlayer.Play("Skate_HandsBack", 0.1, 1.2f);
        }
        else if (Player.InputDir.X < 0)
        {
            AnimationPlayer.Play("Skate_LeftHand", 0.1);
        }
        else if (Player.InputDir.X > 0)
        {
            AnimationPlayer.Play("Skate_RightHand", 0.1);
        }
        else
        {
            AnimationPlayer.Play("Skate", 0.1);
        }
    }

    public void PlayWalkAnimation()
    {
        Player.ControlTrusters(false);
        if (Player.MovementSpeed <= 3)
        {
            DetermineWalkAnimation();
        }
        else
        {
            DetermineRunAnimation();
        }
    }

    public void DetermineWalkAnimation()
    {
        if(Player.InputDir.X < -0.5)
        {
            AnimationPlayer.Play("Strafe_Left", 0.1);
        }
        else if (Player.InputDir.X > 0.5)
        {
            AnimationPlayer.Play("Strafe_Right", 0.1);
        }
        else
        {
            AnimationPlayer.Play("Walk_Forward", 0.1);
        }
    }

    public void DetermineRunAnimation()
    {
        if (Player.InputDir.X < -0.5)
        {
            AnimationPlayer.Play("Strafe_Left", 0.1, 1.2f);
        }
        else if (Player.InputDir.X > 0.5)
        {
            AnimationPlayer.Play("Strafe_Right", 0.1, 1.2f);
        }
        else
        {
            AnimationPlayer.Play("Run", 0.1);
        }
    }
}
