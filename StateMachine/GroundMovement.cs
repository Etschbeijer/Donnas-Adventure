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
        PlayAnimation();
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!Player.IsOnFloor())
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));

            return;
        }

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        PlayAnimation();

        Player.Velocity = Player.PlayerVelocity;

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
        if (Player.CurrentHealth <= 0)
        {
            StateMachine.TransitionTo(nameof(Lose));
        }

        if (Player.HasWon)
        {
            StateMachine.TransitionTo(nameof(Win));
        }
    }

    public void OnHit()
    {
        StateMachine.TransitionTo(nameof(GetHit));
    }

    public void PlayAnimation()
    {
        //Play animation
        if (Player.MovementSpeed < 3)
        {
            Player.AnimationPlayer.Play("Walk_Forward", 0.1);
            Player.ControlTrusters(false);
        }
        else if (Player.MovementSpeed <= 6)
        {
            Player.AnimationPlayer.Play("Run", 0.1);
            Player.ControlTrusters(false);
        }
        else
        {
            Player.AnimationPlayer.Play("Skate", 0.1);
            Player.ControlTrusters(true);
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
