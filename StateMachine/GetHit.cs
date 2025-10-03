using System;
using Godot;
using Godot.Collections;

public partial class GetHit : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void Enter(Dictionary msg = null)
    {
        AnimationPlayer.Play("Hit_Front", 0.1);
        Player.CanInput = false;

        Player.ControlHurtBoxes(false);

        Player.CurrentHealth -= 1;

        if (Player.CurrentHealth <= 0)
        {
            StateMachine.TransitionTo(nameof(Lose));
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!AnimationPlayer.IsPlaying())
        {
            if (Player.IsOnFloor())
            {
                if (Math.Abs(Player.GetInputDirection().Length()) == 0)
                {
                    CallDeferred(nameof(Player.ControlHurtBoxes), true);
                    Player.IsHurt = false;
                    Player.CanInput = true;
                    StateMachine.TransitionTo(nameof(Idle));
                }
                else
                {
                    CallDeferred(nameof(Player.ControlHurtBoxes), true);
                    Player.IsHurt = false;
                    Player.CanInput = true;
                    StateMachine.TransitionTo(nameof(GroundMovement));
                }
            }
            else
            {
                CallDeferred(nameof(Player.ControlHurtBoxes), true);
                Player.IsHurt = false;
                Player.CanInput = true;
                StateMachine.TransitionTo(nameof(AirDropMovement));
            }
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
}
