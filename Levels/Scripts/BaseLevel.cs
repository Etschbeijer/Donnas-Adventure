using Godot;
using System;

public partial class BaseLevel : Node3D
{
    [Export]
    public string LevelId;
    
    public Hud hud;

    public Node3D NumberOfTreats;

    Global global;

    SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        NumberOfTreats = GetNode<Node3D>("Treats");
        hud = GetNode<Hud>("HUD");
        var labelTreats = hud.LabelTreats;
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        //Set initial values
        int treatCount = NumberOfTreats.GetChildCount();
        global.SetCurrentTreats(0);
        global.SetNumberOfTreats(treatCount);
        labelTreats.Text = global.CollectedTreats.ToString();
        global.SetLevelId(LevelId);

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
