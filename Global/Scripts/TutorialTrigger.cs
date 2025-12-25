using Godot;
using System;

public partial class TutorialTrigger : Area3D
{
	[Export]
	public Hud Hud;

    [Export]
    public string TextKeyboard;

    [Export]
    public string TextGamepad;

    [Export]
    public float Time = 3f;

    Global global;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
    }

    public void OnTutorialEntered(CharacterBody3D body)
    {
        var player = body as PlayerCharacter;

        var text = global.IsControllerInput ? TextGamepad : TextKeyboard;

        if (player != null && Hud != null)
        {
			Hud.UpdateTutorial(text, Time);
        }
    }
}
