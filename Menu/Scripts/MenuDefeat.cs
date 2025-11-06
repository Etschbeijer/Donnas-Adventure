using Godot;
using System;

public partial class MenuDefeat : Control
{
    Global global;
    SoundManager soundManager;

    TextureRect deathBySlimes;
    TextureRect deathByDrop;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
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
    }

    public async void OnButtonPlayAgainPressed()
    {
        await soundManager.PlaySoundPlay();

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        await soundManager.PlaySoundExit();

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
