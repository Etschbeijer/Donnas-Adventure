using Godot;

public partial class MenuTitle : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    Button playButton;
    Button level1Button;
    Button level2Button;

    VBoxContainer buttons;
    VBoxContainer levelSelect;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        buttons = GetNode<VBoxContainer>("Buttons");
        levelSelect = GetNode<VBoxContainer>("LevelSelect");
        playButton = buttons.GetNode<Button>("ButtonPlay");
        level1Button = levelSelect.GetNode<Button>("ButtonLevel1");
        level2Button = levelSelect.GetNode<Button>("ButtonLevel2");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //The second level scene may not exist yet
        level2Button.Disabled = !ResourceLoader.Exists(LevelCatalog.Level2);

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

        UiAdapter.SetMouseUi(this, global, isControllerInput, levelSelect.Visible ? level1Button : playButton);

        if (levelSelect.Visible && @event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            OnButtonBackPressed();
        }
    }

    public void OnButtonPlayPressed()
    {
        soundManager.PlaySoundClick();

        buttons.Hide();
        levelSelect.Show();
        level1Button.GrabFocus();
    }

    public void OnButtonBackPressed()
    {
        soundManager.PlaySoundClick();

        levelSelect.Hide();
        buttons.Show();
        playButton.GrabFocus();
    }

    public void OnButtonLevel1Pressed() => StartLevel(LevelCatalog.Level1);

    public void OnButtonLevel2Pressed() => StartLevel(LevelCatalog.Level2);

    async void StartLevel(string scenePath)
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundPlay();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile(scenePath);
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
