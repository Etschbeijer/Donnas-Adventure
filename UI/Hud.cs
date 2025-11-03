using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public partial class Hud : CanvasLayer
{
    public Label LabelTreats;
    public Label LabelTime;
    public Label LabelHealth;
    public Label LabelLifes;
    public Label LabelTutorial;

    public TextureProgressBar Health;
    public TextureProgressBar SpeedGauge;

    bool isDisplayingTutorial = false;

    Queue<(string text, float time)> tutorialQueue = new Queue<(string, float)>();

    Global global;

    Panel panelLife;
    Panel panelTime;
    Panel panelSpeed;
    Panel panelTreats;
    Panel panelTutorial;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");

        panelLife = GetNode<Panel>("PanelLife");
        panelTime = GetNode<Panel>("PanelTime");
        panelSpeed = GetNode<Panel>("PanelSpeed");
        panelTreats = GetNode<Panel>("PanelTreats");
        panelTutorial = GetNode<Panel>("PanelTutorial");

        LabelTime = panelTime.GetNode<Label>("LabelTime");
        LabelLifes = panelLife.GetNode<Label>("LabelLifes");
        LabelHealth = panelLife.GetNode<Label>("LabelHealth");;
        LabelTreats = panelTreats.GetNode<Label>("LabelTreats");
        LabelTutorial = panelTutorial.GetNode<Label>("LabelTutorial");

        Health = panelLife.GetNode<TextureProgressBar>("TextureProgressBar");
        SpeedGauge = panelSpeed.GetNode<TextureProgressBar>("TextureProgressBar");
        

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
        Health.Value = healthValue;
        LabelHealth.Text = healthValue.ToString();
    }

    public void UpdateSpeedGauge(float speedGauge)
    {
        SpeedGauge.Value = speedGauge;
    }

    public void UpdateLifes(int lifes)
    {
        LabelLifes.Text = lifes.ToString();
    }

    public void UpdateTutorial(string text, float time = 3f)
    {
        tutorialQueue.Enqueue((text, time));

        if (!isDisplayingTutorial)
        {
            displayTutorialAsync();
        }
    }

    private async Task displayTutorialAsync()
    {
        isDisplayingTutorial = true;

        while (tutorialQueue.Count > 0)
        {
            var (text, time) = tutorialQueue.Dequeue();

            panelTutorial.Show();
            LabelTutorial.Text = text;

            var timer = GetTree().CreateTimer(time);
            await ToSignal(timer, Timer.SignalName.Timeout);

            panelTutorial.Hide();
            LabelTutorial.Text = "";
        }

        isDisplayingTutorial = false;
    }
}
