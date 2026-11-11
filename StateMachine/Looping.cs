using Godot;
using Godot.Collections;

public partial class Looping : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void Enter(Dictionary msg = null)
    {
        AnimationPlayer.Play("Skate_HandsBack", 0.1, 1.2f);
        Player.ControlTrusters(true);
    }

    public override void PhysicsUpdate(double delta)
    {
        //PlayerCharacter moves the player along the rail.
        if (!Player.IsOnRail)
        {
            StateMachine.TransitionTo(nameof(AirDropMovement));
        }
    }

    public override void Update(double delta)
    {
        if (Player.ShallRespawn() || Player.HasLost())
        {
            Player.AbortRail();
            StateMachine.TransitionTo(nameof(Despawn));
        }

        if (Player.HasWon)
        {
            Player.AbortRail();
            StateMachine.TransitionTo(nameof(Win));
        }
    }

    public override void Exit()
    {
        if (Player.IsOnRail)
        {
            Player.AbortRail();
        }
    }
}
