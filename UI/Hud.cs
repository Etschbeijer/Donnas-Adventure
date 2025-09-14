using Godot;
using System;

public partial class Hud : CanvasLayer
{
	Global global;
    Label labelTreats;
    Label labelTime;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
		global = GetNode<Global>("/root/Global");
        labelTreats = GetNode<Label>("LabelTreats");
        labelTime = GetNode<Label>("LabelTime");

        //Set initial values
        labelTreats.Text = global.collectedTreats.ToString();
        labelTime.Text = $"{global.minutes:D2}:{global.seconds:D2}";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
    }

    public void updateLabelTreats()
    {
        labelTreats.Text = global.collectedTreats.ToString();
    }

    public void updateLabelTime()
    {
        labelTime.Text = $"{global.minutes:D2}:{global.seconds:D2}";
    }
}
