using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Spawn : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public Global Global;

    public SoundManager SoundManager;

    bool playSound = true;

    public override async void Enter(Dictionary msg = null)
    {
        //Get nodes
        Global = GetNode<Global>("/root/Global");
        SoundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        Player.DissolveValue = -1f;
        Player.TimeSinceLeftFloor = 0f;
        Player.IsHurt = false;
        playSound = true;

        //Call important methods
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Player.ControlTrusters(false);
        Player.ControlFeetCollision(true, false);
        Player.ControlHurtBoxes(true);
        Player.ReplaceWithDissolveShader();

        Player.Respawn();

        Player.TransitionScreen.FadeToNormal();
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.Dissolve((float)delta, 1f, -1f);

        if (Player.DissolveValue > 0f && playSound)
        {
            Player.PlaySoundDissolve();
            playSound = false;
        }

        if (!AnimationPlayer.IsPlaying() && !Player.IsOnFloor())
        {
            AnimationPlayer.Play("Jump_Drop", 0.1);
        }

        if (Player.DissolveValue >= 0.8f)
        {
            Player.ReplaceWithStandardMaterial();
            Player.CanInput = true;

            if (Input.IsActionJustPressed("Jump") && !Player.IsOnFloor() && Player.HasDoubleJump)
            {
                Player.HasDoubleJump = false;
                Player.PlayerVelocity.Y = 0;
                StateMachine.TransitionTo(nameof(AirRiseMovement));
            }

            if (Input.IsActionJustPressed("Stomp") || (Input.IsActionPressed("Stomp")))
            {
                Player.PlayerVelocity.Y = 0;
                StateMachine.TransitionTo(nameof(Stomp));
            }

            if (Player.IsOnFloor())
            {
                Player.HasDoubleJump = true;
                Player.FireExplosion();
                Player.PlayerVelocity.Y = 0f;
                Player.Velocity = Player.PlayerVelocity;
                Player.ControlFeetCollision(true, false);
                Player.ControlSaltoCollision(false);

                if (Player.InputDir == Vector2.Zero)
                {
                    StateMachine.TransitionTo(nameof(Idle));
                }
                else
                {
                    Player.PlaySoundLandOnGrass();
                    StateMachine.TransitionTo(nameof(GroundMovement));
                }
            }
            else
            {
                Player.HasDoubleJump = true;
                StateMachine.TransitionTo(nameof(AirDropMovement));
            }
        }
    }
}
