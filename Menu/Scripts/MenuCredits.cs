using Godot;
using System;

public partial class MenuCredits : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        transitionScreen.FadeToNormal();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        UiAdapter.SetMouseUi(global.IsControllerInput);
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }
}
