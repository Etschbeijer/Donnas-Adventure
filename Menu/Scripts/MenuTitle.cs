using Godot;

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
        soundManager.EnsureMenuMusic();

        transitionScreen.FadeToNormal();

        if (global.IsControllerInput)
        {
            playButton.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        UiAdapter.SetMouseUi(this, global, isControllerInput, playButton);
    }

    public async void OnButtonPlayPressed()
	{
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundPlay();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void OnButtonScoresPressed() => OpenMenu("res://Menu/Nodes/menu_highscores.tscn");

    public void OnButtonSettingsPressed() => OpenMenu("res://Menu/Nodes/menu_settings_sound.tscn");

    public void OnButtonCreditsPressed() => OpenMenu("res://Menu/Nodes/menu_credits.tscn");

    void OpenMenu(string scenePath)
    {
        soundManager.PlaySoundClick();

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile(scenePath);
    }

    public async void OnButtonExitPressed()
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }
}
