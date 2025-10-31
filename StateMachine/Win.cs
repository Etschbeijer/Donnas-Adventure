using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class Win : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public async override void Enter(Dictionary msg = null)
    {
        Player.CanInput = false;

        Player.Global.StopTimer();

        Player.ControlHurtBoxes(false);

        Player.PlayerVelocity = Vector3.Zero;

        await Won();
        await LoadWinScene("res://Menu/Nodes/menu_win.tscn");
    }

    public async Task Won()
    {
        AnimationPlayer.Play("Win", 0.1);
        Player.SoundManager.PlaySoundWin();
        Player.ControlTrusters(false);

        await ToSignal(AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    public async Task LoadWinScene(string nextScene)
    {
        var sceneTree = GetTree();

        var timer = sceneTree.CreateTimer(0.1);
        await ToSignal(timer, Timer.SignalName.Timeout);

        if (sceneTree != null)
        {
            Player.LoadScene(sceneTree, nextScene);
        }
    }
}
