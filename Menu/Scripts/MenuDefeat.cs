using Godot;
using System;

public partial class MenuDefeat : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    TextureRect deathBySlimes;
    TextureRect deathByDrop;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        deathByDrop = GetNode<TextureRect>("DeathByDrop");
        deathBySlimes = GetNode<TextureRect>("DeathBySlimes");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        if (soundManager.IsMusicPuppyPlayingInTheGarden2DPlaying())
        {
            soundManager.StopMusicPuppyPlayingInTheGarden2D();
        }

        if (!soundManager.IsMusicProject2Marioish2DPlaying())
        {
            soundManager.PlayMusicProject2Marioish2D();
        }

        if (global.Deathypes == Deathypes.Slimes)
        {
            ShowDeathBySlimes();
        }
        else if (global.Deathypes == Deathypes.Drop)
        {
            ShowDeathByDrop();
        }

        transitionScreen.FadeToNormal();
    }

    public override void _Process(double delta)
    {
        UiAdapter.SetMouseUi(global.IsControllerInput);
    }

    public async void OnButtonPlayAgainPressed()
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundPlay();

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public async void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        transitionScreen.FadeToBlack();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }

    public void ShowDeathByDrop()
    {
        deathByDrop.Show();
    }

    public void ShowDeathBySlimes()
    {
        deathBySlimes.Show();
    }
}
