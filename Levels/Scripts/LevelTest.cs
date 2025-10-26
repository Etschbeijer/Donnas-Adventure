using Godot;
using System;

public partial class LevelTest : Node3D
{
    Global global;

    public Node3D numberOfTreats;

    public Hud hud;

    SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        numberOfTreats = GetNode<Node3D>("Treats");
        hud = GetNode<Hud>("HUD");
        var labelTreats = hud.LabelTreats;
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        int treatCount = numberOfTreats.GetChildCount();
        global.SetCurrentTreats(0);
        global.SetNumberOfTreats(treatCount);
        labelTreats.Text = global.CollectedTreats.ToString();

        //Call important methods
        soundManager.StopMusicProject2Marioish2D();
        soundManager.PlayMusicPuppyPlayingInTheGarden2D();
        global.ResetTimer();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        hud.UpdateLabelTime();
    }
}
