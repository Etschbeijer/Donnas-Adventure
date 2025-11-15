using Godot;
using System;
using System.Collections.Generic;

public partial class MenuPause : Control
{
    Global global;

    SoundManager soundManager;

    TransitionScreen transitionScreen;

    bool isPaused = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("Pause"))
        {
            CallDeferred(nameof(SetPause));
        }
    }

    public void SetPause()
    {
        if (isPaused)
        {
            Hide();
            GetTree().Paused = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Show();
            GetTree().Paused = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        isPaused = !isPaused;
    }

    public void OnButtonResumePressed()
    {
        GetTree().Paused = false;
        soundManager.PlaySoundClick();
        CallDeferred(nameof(SetPause));
    }

    public async void OnButtonMenuPressed()
    {
        GetTree().Paused = false;
        soundManager.PlaySoundClick();

        transitionScreen.FadeToBlack();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        GetTree().Paused = false;

        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }
}
