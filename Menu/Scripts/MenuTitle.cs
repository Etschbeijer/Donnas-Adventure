using Godot;

public partial class MenuTitle : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    bool isStarting;

    Button playButton;
    Button level1Button;

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

        UiAdapter.SetMouseUi(this, global, isControllerInput, levelSelect.Visible ? level1Button : playButton);

        if (levelSelect.Visible && !isStarting && @event.IsActionPressed("ui_cancel"))
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
        if (global.IsControllerInput)
        {
            level1Button.GrabFocus();
        }
    }

    public void OnButtonBackPressed()
    {
        if (isStarting)
        {
            return;
        }

        soundManager.PlaySoundClick();

        levelSelect.Hide();
        buttons.Show();
        if (global.IsControllerInput)
        {
            playButton.GrabFocus();
        }
    }

    public void OnButtonLevel1Pressed() => StartLevel(LevelCatalog.Level1);

    public void OnButtonLevel2Pressed() => StartLevel(LevelCatalog.Level2);

    async void StartLevel(string scenePath)
    {
        if (isStarting)
        {
            return;
        }
        isStarting = true;

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
