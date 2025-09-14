using Godot;
using System;

public partial class Level1 : Node3D
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
        var labelTreats = hud.GetNode<Label>("LabelTreats");
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        int treatCount = numberOfTreats.GetChildCount();
        global.setCurrentTreats(0);
        global.setNumberOfTreats(treatCount);
        labelTreats.Text = global.collectedTreats.ToString();

        //Call important methods
        soundManager.stopMusicProject2Marioish2D();
        soundManager.playMusicPuppyPlayingInTheGarden3D();
        global.resetTimer();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        hud.updateLabelTime();
    }
}
