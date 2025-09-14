using Godot;
using System;

public partial class MenuCredits : Control
{
    SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void onButtonMenuPressed()
    {
        soundManager.playSoundMainMenu();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }
}
