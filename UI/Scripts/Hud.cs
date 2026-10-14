using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Hud : CanvasLayer
{
    public Label LabelTime;
    public Label LabelLifes;
    public Label LabelTreats;
    public Label LabelHealth;
    public Label LabelTutorial;

    public TextureProgressBar Health;
    public TextureProgressBar SpeedGauge;

    bool isDisplayingTutorial = false;
    bool isExiting = false;
    int displayedMinutes = -1;
    int displayedSeconds = -1;

    Queue<(string text, float time)> tutorialQueue = new Queue<(string, float)>();

    Global global;

    Panel panelLife;
    Panel panelTime;
    Panel panelSpeed;
    Panel panelTreats;
    Panel panelTutorial;
    Panel panelSpecialTreats;

    TextureRect textureMeat;
    TextureRect textureCheese;
    TextureRect textureCookie;

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
        panelSpecialTreats = GetNode<Panel>("PanelSpecialTreats");

        textureMeat = panelSpecialTreats.GetNode<TextureRect>("TextureMeat");
        textureCheese = panelSpecialTreats.GetNode<TextureRect>("TextureCheese");
        textureCookie = panelSpecialTreats.GetNode<TextureRect>("TextureCookie");
        
        LabelTime = panelTime.GetNode<Label>("LabelTime");
        LabelLifes = panelLife.GetNode<Label>("LabelLifes");
        LabelHealth = panelLife.GetNode<Label>("LabelHealth");
        LabelTreats = panelTreats.GetNode<Label>("LabelTreats");
        LabelTutorial = panelTutorial.GetNode<Label>("LabelTutorial");

        Health = panelLife.GetNode<TextureProgressBar>("TextureProgressBar");
        SpeedGauge = panelSpeed.GetNode<TextureProgressBar>("TextureProgressBar");
        

        //Set initial values
        LabelTreats.Text = global.CollectedTreats.ToString();
        UpdateLabelTime();
    }

    public override void _ExitTree()
    {
        isExiting = true;
        tutorialQueue.Clear();
    }

    public void UpdateLabelTreats()
    {
        LabelTreats.Text = global.CollectedTreats.ToString();
    }

    public void UpdateLabelTime()
    {
        if (displayedMinutes == global.Minutes && displayedSeconds == global.Seconds)
            return;

        displayedMinutes = global.Minutes;
        displayedSeconds = global.Seconds;
        LabelTime.Text = $"{displayedMinutes:D2}:{displayedSeconds:D2}";
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
        if (isExiting)
            return;

        tutorialQueue.Enqueue((text, time));

        if (!isDisplayingTutorial)
        {
            _ = displayTutorialAsync();
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

            if (!IsInstanceValid(this) || isExiting || !IsInsideTree())
                return;

            panelTutorial.Hide();
            LabelTutorial.Text = "";
        }

        isDisplayingTutorial = false;
    }

    public void ShowCheese()
    {
        textureCheese.ShowBehindParent = false;
    }

    public void ShowCookie()
    {
        textureCookie.ShowBehindParent = false;
    }

    public void ShowMeat()
    {
        textureMeat.ShowBehindParent = false;
    }
}
