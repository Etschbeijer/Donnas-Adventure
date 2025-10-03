using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class Attack : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public string Animation { get; set; }

    [Export]
    public string NextState { get; set; }

    private bool actionPressed = false;

    public override void Enter(Dictionary msg = null)
    {
        //Player.AnimationNodeState.Travel(Animation);
        AnimationPlayer.Play(Animation, 0.1);
        Player.CanInput = false;
        actionPressed = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        //float velocityY = (float)(Player.Velocity.Y + -Player.Gravity * delta);

        if (Player.IsOnFloor())
        {
            //Player.Velocity = Player.Velocity.Lerp(new Vector3(), (float)(Player.AttackFriction * delta));
        }

        Player.Velocity = new Vector3(Player.Velocity.X, Player.Velocity.Y, Player.Velocity.Z);

        if (Input.IsActionJustPressed("Attack"))
        {
            actionPressed = true;
        }

        //if(Player.IsOnFloor())
        //{
        //    var viewPosition = new Vector3(mousePosition.X, Player.Model.GlobalPosition.Y, mousePosition.Z);
        //    Player.LookAt(viewPosition, Vector3.Up);
        //}
        //else
        //{
        //    var viewPosition = new Vector3(mousePosition.X, Player.Model.Position.Y, mousePosition.Z);
        //    Player.LookAt(viewPosition, Vector3.Up);
        //}

        if (NextState is not null && NextState.Any() && Player.CanInput && actionPressed)
        {
            StateMachine.TransitionTo(NextState);
        }

        if (!AnimationPlayer.IsPlaying())
        {
            if(Player.GetInputDirection().Length() == 0)
            {
                StateMachine.TransitionTo("Idle");
            }
            else
            {
                StateMachine.TransitionTo("Run");
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
    public void controlHitbox(Area3D hitbox, bool onOff)
    {
        var collisionBox = hitbox.GetChild<CollisionShape3D>(0);

        hitbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hitbox.SetCollisionLayerValue(7, onOff);
        hitbox.SetCollisionMaskValue(8, onOff);
    }
}
