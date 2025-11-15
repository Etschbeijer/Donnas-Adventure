using Godot;
using System;

public partial class TransitionScreen : CanvasLayer
{
    public AnimationPlayer AnimationPlayer { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

	public void FadeToBlack()
    {
        Show();
        AnimationPlayer.Play("fade_black");
    }

    public void FadeToNormal()
    {
        AnimationPlayer.Play("fade_normal");

        if (!AnimationPlayer.IsPlaying())
        {
            Hide();
        }
    }
}
