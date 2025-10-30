using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class WallHanging : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    Vector3 wallNormal;

    public override void Enter(Dictionary msg = null)
    {
        Player.TimeOnWall = 0f;

        Player.ShallBounce = false;
        Player.HasDoubleJump = true;

        Player.PlayerVelocity = Vector3.Zero;
        Player.Velocity = Player.PlayerVelocity;

        FaceToWall();

        AnimationPlayer.Play("Wall_Idle", 0.1);
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Player.TimeOnWall <= Player.WallTime && Player.IsOnWallOnly())
        {
            Player.TimeOnWall += (float)delta;
            AnimationPlayer.Play("Wall_Idle", 0.1);
        }
        else
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (Player.IsHurt)
        {
            Player.ControlFeetCollision(true, false);
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (Input.IsActionJustPressed("Jump"))
        {
            Dictionary msg = new Dictionary();
            msg["doJump"] = true;

            Vector3 jumpDirection = (Vector3.Up + wallNormal).Normalized();
            Player.PlayerVelocity = jumpDirection * Player.JumpVelocity * 0.5f;
            StateMachine.TransitionTo(nameof(WallJump), msg);
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

    public void FaceToWall()
    {
        KinematicCollision3D wallCollission = Player.GetLastSlideCollision();
        wallNormal = wallCollission.GetNormal().Normalized();
        Vector3 lookTarget = Player.GlobalTransform.Origin + wallNormal;
        Player.LookAt(lookTarget, Vector3.Up);
    }
}
