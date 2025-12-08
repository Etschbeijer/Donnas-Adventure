using Godot;
using System;
using System.Collections.Generic;

public partial class MenuPause : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    Button resumeButton;

    bool isPaused = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        resumeButton = GetNode<VBoxContainer>("VBoxContainer").GetNode<Button>("ButtonResume");

        //Call important methods
        GetViewport().GuiReleaseFocus();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("Pause"))
        {
            CallDeferred(nameof(SetPause));
        }

        if (isPaused)
        {
            UiAdapter.SetMouseUi(global.IsControllerInput);
        }
    }

    public void SetPause()
    {
        GetViewport().GuiReleaseFocus();
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
            UiAdapter.SetMouseUi(global.IsControllerInput);
            resumeButton.GrabFocus();
        }
        isPaused = !isPaused;
    }

    public override void _Input(InputEvent @event)
    {
        global.IsControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;
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
