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
        Player.TimeonWall = 0f;

        Player.ShallBounce = false;
        Player.HasDoubleJump = true;

        Player.PlayerVelocity = Vector3.Zero;

        FaceToWall();

        AnimationPlayer.Play("Wall_Idle", 0.1);
    }

    public override void PhysicsUpdate(double delta)
    {
        if(Player.TimeonWall < Player.WallTime && Player.IsOnWallOnly())
        {
            Player.PlayerVelocity.Y = 0f;
            Player.Velocity = Player.PlayerVelocity;
            Player.TimeonWall += (float)delta;
        }
        else
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnWallOnly())
        {
            AnimationPlayer.Play("Wall_Idle", 0.1);
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
            Player.PlayerVelocity = jumpDirection * Player.JumpVelocity;
            StateMachine.TransitionTo(nameof(WallJump), msg);
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

    public void FaceToWall()
    {
        KinematicCollision3D wallCollission = Player.GetLastSlideCollision();
        wallNormal = wallCollission.GetNormal().Normalized();

        Vector3 lookTarget = Player.GlobalTransform.Origin + wallNormal;
        Player.LookAt(lookTarget);
    }
}
