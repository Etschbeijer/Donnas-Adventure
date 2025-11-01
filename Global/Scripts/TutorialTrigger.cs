using Godot;
using System;

public partial class TutorialTrigger : Area3D
{
	[Export]
	public Hud Hud;

    [Export]
    public string Text;

    [Export]
    public float Time = 3f;

    public void OnTutorialEntered(CharacterBody3D body)
    {
        var player = body as PlayerCharacter;

        if (player != null && Hud != null)
        {
			Hud.UpdateTutorial(Text, Time);
        }
    }
}
