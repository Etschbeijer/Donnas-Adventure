using System;
using Godot;
using Godot.Collections;

public partial class Despawn : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public Global Global;

    public SoundManager SoundManager;

    public override async void Enter(Dictionary msg = null)
    {
        //Get nodes
        Global = GetNode<Global>("/root/Global");
        SoundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        Player.DissolveValue = -1f;
        Player.TimeSinceLeftFloor = 0f;

        //Call important methods
        Player.ControlTrusters(false);
        Player.ControlFeetCollision(true, false);
        Player.ReplaceWithDissolveShader();
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.Dissolve((float)delta, -1f, 1f);

        if(Player.DissolveValue > 0 && !Player.TransitionScreen.AnimationPlayer.IsPlaying())
        {
            Player.PlaySoundDissolve();
            Player.TransitionScreen.FadeToBlack();
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Drop", 0.1);
        }
        else if (!AnimationPlayer.IsPlaying() && Player.IsOnFloor())
        {
            AnimationPlayer.Play("Stay_Defeated", 0.1);
        }

        if (Player.DissolveValue >= 0.8f)
        {
            if (Player.HasLost())
            {
                StateMachine.TransitionTo(nameof(Lose));
            }
            else
            {
                StateMachine.TransitionTo(nameof(Spawn));
            } 
        }
    }
}
