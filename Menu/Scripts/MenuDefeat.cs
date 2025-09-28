using Godot;
using System;

public partial class MenuDefeat : Control
{
    SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        soundManager.StopMusicPuppyPlayingInTheGarden3D();
        soundManager.PlayMusicProject2Marioish2D();
    }

    public async void onButtonPlayAgainPressed()
    {
        await soundManager.PlaySoundPlay();

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void onButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void onButtonExitPressed()
    {
        await soundManager.PlaySoundExit();

        GetTree().Quit();
    }
}
