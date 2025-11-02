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

        if (Player.InputDir.X < 0)
        {
            Player.AnimationPlayer.Play("Skate_LeftHand", 0.1);
        }
        else if (Player.InputDir.X > 0)
        {
            Player.AnimationPlayer.Play("Skate_RightHand", 0.1);
        }
        else
        {
            Player.AnimationPlayer.Play("Skate", 0.1);
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
        if(Player.InputDir.X < 0)
        {
            Player.AnimationPlayer.Play("Strafe_Left", 0.1);
        }
        else if (Player.InputDir.X > 0)
        {
            Player.AnimationPlayer.Play("Strafe_Right", 0.1);
        }
        else
        {
            Player.AnimationPlayer.Play("Walk_Forward", 0.1);
        }
    }

    public void DetermineRunAnimation()
    {
        if (Player.InputDir.X < 0)
        {
            Player.AnimationPlayer.Play("Strafe_Left", 0.1, 1.2f);
        }
        else if (Player.InputDir.X > 0)
        {
            Player.AnimationPlayer.Play("Strafe_Right", 0.1, 1.2f);
        }
        else
        {
            Player.AnimationPlayer.Play("Run", 0.1);
        }
    }

    private void DetermineLocomotionAnimation(Vector3 mousePosition)
    {
        if (mousePosition.Z > Player.GlobalPosition.Z)
        {
            Player.AnimationTree.Set("parameters/IWR/blend_position", new Vector2(-Player.Velocity.X, Player.Velocity.Z) / Player.MovementSpeed);
        }
        else
        {
            Player.AnimationTree.Set("parameters/IWR/blend_position", new Vector2(Player.Velocity.X, -Player.Velocity.Z) / Player.MovementSpeed);
        }
    }
}
