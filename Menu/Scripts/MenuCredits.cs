using Godot;
using System;

public partial class MenuCredits : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    Button menuButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        menuButton = GetNode<VBoxContainer>("Buttons").GetNode<Button>("ButtonMenu");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        transitionScreen.FadeToNormal();

        if (global.IsControllerInput)
        {
            menuButton.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        UiAdapter.SetMouseUi(this, global, isControllerInput, menuButton);
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }
}
