using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuTitle : Control
{
    private SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");

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
    }

    public async void OnButtonPlayPressed()
	{
        await soundManager.PlaySoundPlay();

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_test.tscn");
    }

    public void OnButtonScoresPressed()
    {
        soundManager.PlaySoundClick();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_highscores.tscn");
    }

    public void OnButtonSettingsPressed()
    {
        soundManager.PlaySoundClick();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_settings_sound.tscn");
    }

    public void OnButtonCreditsPressed()
    {
        soundManager.PlaySoundClick();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_credits.tscn");
    }

    public async void OnButtonExitPressed()
    {
        await soundManager.PlaySoundExit();

        GetTree().Quit();
    }
}
