using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class AirDropMovement : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    bool isGravityActive = true;

    public override async void Enter(Dictionary msg = null)
    {
        Player.ControllFeetCollision(false, true);

        if (StateMachine.PreviousState.Name == nameof(AirRiseMovement))
        {
            AnimationPlayer.Play("Jump_Salto", 0.1, 2);
            Player.ControlTrusters(false);
            await StopInAir(0.1);
        }
        else
        {
            AnimationPlayer.Play("Jump_Drop", 0.1);
            Player.ControlTrusters(false);
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.TimeSinceLeftFloor += (float)delta;


        if (Input.IsActionJustPressed("Jump") && Player.TimeSinceLeftFloor < Player.CoyoteTime)
        {
            Dictionary msg = new Dictionary();
            msg["doJump"] = true;

            Player.PlayerVelocity.Y = 0;
            StateMachine.TransitionTo(nameof(AirRiseMovement), msg);
        }
        else if (Input.IsActionJustPressed("Jump") && !Player.IsOnFloor() && Player.HasDoubleJump)
        {
            Player.HasDoubleJump = false;
            Player.PlayerVelocity.Y = 0;
            StateMachine.TransitionTo(nameof(AirRiseMovement));
        }

        if (isGravityActive)
        {
            CalculateGravityVelocity((float)delta); 
        }

        Player.CalculateVelocity(Player.InputDir, Player.Direction, delta);

        if (Player.IsHurt)
        {
            StateMachine.TransitionTo(nameof(GetHit));
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Drop", 0.1);
        }

        //if (Input.IsActionJustPressed("Attack"))
        //{
        //    StateMachine.TransitionTo("Attack1");
        //}

        if (Player.IsOnFloor())
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
            Player.PlayerVelocity += Player.Gravity * delta * 2;
            Player.TimeSinceLeftFloor += delta;
        }
    }

    public bool IsAtTop(float delta)
    {
        return (Player.PlayerVelocity <= (Player.Gravity * delta));
    }

    public void OnHit()
    {
        Player.PlayerVelocity.Y = 0f;
        Player.Velocity = Player.PlayerVelocity;
        StateMachine.TransitionTo(nameof(GetHit));
    }

    public async Task StopInAir(double time)
    {
        isGravityActive = false;

        Player.PlayerVelocity.Y = 0;
        Player.Velocity = Player.PlayerVelocity;

        var sceneTree = GetTree();
        var timer = sceneTree.CreateTimer(time);
        await ToSignal(timer, Timer.SignalName.Timeout);

        isGravityActive = true;
    }
}
