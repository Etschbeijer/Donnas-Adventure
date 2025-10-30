using System;
using Godot;
using Godot.Collections;

public partial class Idle : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer {  get; set; }

	public override void Enter(Dictionary msg = null)
	{
        Player.TimeOnWall = 0f;
        Player.TimeSinceLeftFloor = 0;

        if (StateMachine.PreviousState != null && (StateMachine.PreviousState.Name == nameof(AirRiseMovement) || StateMachine.PreviousState.Name == nameof(AirDropMovement) || StateMachine.PreviousState.Name == nameof(WallJump)))
        {
            AnimationPlayer.Play("Jump_Land", 0.1);
        }
        else if (StateMachine.PreviousState != null && (StateMachine.PreviousState.Name == nameof(GroundMovement)) && Player.MovementSpeed >= 6f)
        {
            AnimationPlayer.Play("Run_Stop", 0.1);
        }
        else
        {
            AnimationPlayer.Play("Idle", 0.1);
        }
    }

    public override void PhysicsUpdate(double delta)
	{
        CalculateVelocity(Player.InputDir, delta);

        if (!AnimationPlayer.IsPlaying())
        {
            AnimationPlayer.Play("Idle", 0.1);
        }

        if (!Player.IsOnFloor())
		{
			StateMachine.TransitionTo(nameof(AirDropMovement));
			return;
		}

        var velocityY = Player.Velocity.Y;
		Player.Velocity = new Vector3(Player.Velocity.X, velocityY, Player.Velocity.Z);

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
        else if(Math.Abs(Player.GetInputDirection().Length()) != 0)
		{
            StateMachine.TransitionTo(nameof(GroundMovement));
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

    public void CalculateVelocity(Vector2 inputDir, double delta)
    {
        if (inputDir == Vector2.Zero)
        {
            //Reduce movement speed when not pressing a button
            var stopRate =
                (Player.MovementSpeed > 6f)
                ? Player.TopSpeed / Player.TopAccelarationTime
                : Player.TopSpeed / Player.RunAccelarationTime;

            Player.MovementSpeed = Mathf.MoveToward(Player.MovementSpeed, 0f, stopRate * (float)delta);

            Player.PlayerVelocity.X = Mathf.MoveToward(Player.Velocity.X, 0, stopRate * (float)delta);
            Player.PlayerVelocity.Z = Mathf.MoveToward(Player.Velocity.Z, 0, stopRate * (float)delta);
        }
    }
}
