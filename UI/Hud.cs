using Godot;
using System;

public partial class Hud : CanvasLayer
{
    Global global;
    public Label LabelTreats;
    public Label LabelTime;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");

        LabelTreats = GetNode<Label>("LabelTreats");
        LabelTime = GetNode<Label>("LabelTime");

        //Set initial values
        LabelTreats.Text = global.CollectedTreats.ToString();
        LabelTime.Text = $"{global.Minutes:D2}:{global.Seconds:D2}";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
    }

    public void UpdateLabelTreats()
    {
        LabelTreats.Text = global.CollectedTreats.ToString();
    }

    public void UpdateLabelTime()
    {
        LabelTime.Text = $"{global.Minutes:D2}:{global.Seconds:D2}";
    }
}
