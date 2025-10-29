using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class Lose : PlayerState
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public async override void Enter(Dictionary msg = null)
    {
        Player.CanInput = false;

        await Defeated();
        await LoadDefeatScene("res://Menu/Nodes/menu_defeat.tscn");
    }

    public async Task Defeated()
    {
        Player.SoundManager.PlaySoundLose();
        Player.ControlTrusters(false);

        if (StateMachine.PreviousState != null && StateMachine.PreviousState.Name == nameof(GetHit))
        {
            AnimationPlayer.Play("Defeat", 0.1);
            await ToSignal(AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
    }

    public async Task LoadDefeatScene(string nextScene)
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
