using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuTitle : Control
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

    public async void onButtonPlayPressed()
	{
        await soundManager.PlaySoundPlay();

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void onButtonScoresPressed()
    {
        soundManager.PlaySoundClick();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_highscores.tscn");
    }

    public void onButtonCreditsPressed()
    {
        soundManager.PlaySoundClick();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_credits.tscn");
    }

    public async void onButtonExitPressed()
    {
        await soundManager.PlaySoundExit();

        GetTree().Quit();
    }
}
