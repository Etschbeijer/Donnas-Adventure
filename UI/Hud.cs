using Godot;
using System;

public partial class Hud : CanvasLayer
{
    public Label LabelTreats;
    public Label LabelTime;
    public Label LabelHealth;
    public Label LabelLifes;
    
    public TextureProgressBar TextureProgressBar;

    private Panel panelTreats;
    private Panel panelTime;
    private Panel panelLife;

    Global global;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");

        panelTreats = GetNode<Panel>("PanelScore");
        panelTime = GetNode<Panel>("PanelTime");
        panelLife = GetNode<Panel>("PanelLife");

        LabelTreats = panelTreats.GetNode<Label>("LabelTreats");
        LabelTime = panelTime.GetNode<Label>("LabelTime");
        LabelHealth = panelLife.GetNode<Label>("LabelHealth");
        LabelLifes = panelLife.GetNode<Label>("LabelLifes");

        TextureProgressBar = panelLife.GetNode<TextureProgressBar>("TextureProgressBar");

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

    public void UpdateHealthbar(float healthValue)
    {
        TextureProgressBar.Value = healthValue;
        LabelHealth.Text = healthValue.ToString();
    }

    public void UpdateLifes(int lifes)
    {
        LabelLifes.Text = lifes.ToString();
    }
}
