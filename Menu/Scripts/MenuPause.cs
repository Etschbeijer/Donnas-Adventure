using Godot;

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

        resumeButton = GetNode<VBoxContainer>("Buttons").GetNode<Button>("ButtonResume");

        Input.MouseMode = Input.MouseModeEnum.Captured;

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
            if (global.IsControllerInput)
            {
                UiAdapter.ResetGuiHover(this);
                resumeButton.GrabFocus();
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            GetTree().Paused = true;
        }
        isPaused = !isPaused;
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        if(isPaused)
        {
            if (isControllerInput && isControllerInput != global.IsControllerInput)
            {
                UiAdapter.ResetGuiHover(this);
            }
            UiAdapter.SetMouseUi(this, global, isControllerInput, resumeButton);
        }
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
        ProcessMode = Node.ProcessModeEnum.Always;
        transitionScreen.ProcessMode = Node.ProcessModeEnum.Always;
        soundManager.ProcessMode = Node.ProcessModeEnum.Always;

        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }
}
