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

        Player.ControlHurtBoxes(false);

        Player.ReduceHealth();

        Player.PlaySoundOnHit();

        if (Player.CurrentHealth <= 0)
        {
            Player.ReduceLife();
            Player.Hud.UpdateLifes(Player.Lifes);
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
                    Player.ControlHurtBoxes(true);
                    Player.IsHurt = false;
                    StateMachine.TransitionTo(nameof(Idle));
                }
                else
                {
                    Player.ControlHurtBoxes(true);
                    Player.IsHurt = false;
                    StateMachine.TransitionTo(nameof(GroundMovement));
                }
            }
            else
            {
                Player.ControlHurtBoxes(true);
                Player.IsHurt = false;
                StateMachine.TransitionTo(nameof(AirDropMovement));
            }
        }
    }

    public override void Update(double delta)
    {
        if (Player.ShallRespawn() && !AnimationPlayer.IsPlaying())
        {
            Player.Respawn();
        }
        else if (Player.HasLost())
        {
            StateMachine.TransitionTo(nameof(Lose));
        }

        if (Player.HasWon && !AnimationPlayer.IsPlaying())
        {
            StateMachine.TransitionTo(nameof(Win));
        }
    }
}
