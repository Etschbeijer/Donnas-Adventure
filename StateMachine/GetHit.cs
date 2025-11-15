using System;
using Godot;
using Godot.Collections;

public partial class GetHit : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void Enter(Dictionary msg = null)
    {
        Player.ControlHurtBoxes(false);

        Player.ReduceHealth();

        Player.PlaySoundOnHit();

        if (Player.CurrentHealth <= 0)
        {
            AnimationPlayer.Play("Defeat", 0.1);
            Player.PlayerVelocity = Vector3.Zero;
            Player.CanInput = false;
            Player.ReduceLife();
            Player.IsHurt = false;
            Player.Hud.UpdateLifes(Player.Lifes);
        }
        else
        {
            AnimationPlayer.Play("Hit_Front", 0.1);
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!AnimationPlayer.IsPlaying() || (Math.Abs(Player.GetInputDirection().Length()) != 0))
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
        if ((Player.ShallRespawn() || Player.HasLost()) && !AnimationPlayer.IsPlaying() )
        {
            StateMachine.TransitionTo(nameof(Despawn));
        }

        if (Player.HasWon && !AnimationPlayer.IsPlaying())
        {
            StateMachine.TransitionTo(nameof(Win));
        }
    }
}
