using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuTitle : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    Button playButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        playButton = GetNode<VBoxContainer>("Buttons").GetNode<Button>("ButtonPlay");

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

        transitionScreen.FadeToNormal();
    }

    public override void _Process(double delta)
    {
        UiAdapter.SetMouseUi(global.IsControllerInput);
    }

    public override void _Input(InputEvent @event)
    {
        var input = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        if (!input && input != global.IsControllerInput)
        {
            GetViewport().GuiReleaseFocus();
        }

        if (input && input != global.IsControllerInput)
        {
            playButton.GrabFocus();
        }
        global.IsControllerInput = input;
    }

    public async void OnButtonPlayPressed()
	{
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundPlay();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void OnButtonScoresPressed()
    {
        soundManager.PlaySoundClick();

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_highscores.tscn");
    }

    public void OnButtonSettingsPressed()
    {
        soundManager.PlaySoundClick();

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_settings_sound.tscn");
    }

    public void OnButtonCreditsPressed()
    {
        soundManager.PlaySoundClick();

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_credits.tscn");
    }

    public async void OnButtonExitPressed()
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }
}
