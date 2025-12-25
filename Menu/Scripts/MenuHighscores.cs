using Godot;
using System;
using System.Linq;

public partial class MenuHighscores : Control
{
    [Export]
    PackedScene PanelRowScene;
    
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    VBoxContainer buttons;
    VBoxContainer container;

    Button menuButton;
    Button resetHighScoreButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        container = GetNode<VBoxContainer>("VBoxContainer");
        buttons = GetNode<VBoxContainer>("Buttons");
        resetHighScoreButton = buttons.GetNode<Button>("ButtonReset");
        menuButton = buttons.GetNode<Button>("ButtonMenu");

        //Call important methods
        ClearHighScores();
        ShowHighScores();

        transitionScreen.FadeToNormal();

        if (global.IsControllerInput)
        {
            menuButton.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        UiAdapter.SetMouseUi(this, global, isControllerInput, menuButton);
    }

    public void ClearHighScores()
    {
        foreach (Node child in container.GetChildren())
        {
            child.QueueFree();
        }
    }

    private string formatTime(double time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }

    private string toOrdinal(int number)
    {
        int rem100 = number % 100;
        if (rem100 >= 11 && rem100 <= 13)
        {
            return $"{rem100}th";
        }

        switch (number % 10)
        {
            case 1: return $"{number}st";
            case 2: return $"{number}nd";
            case 3: return $"{number}rd";
            default: return $"{number}th";
        }
    }

    public void ShowHighScores()
    {
        var highScores = global.SaveData.PlayerScores;

        var orderedHighscores = highScores.OrderByDescending(playerScore => playerScore.Score).ToArray();

        for (int i = 0; i < orderedHighscores.Length; i++)
        {
            var row = PanelRowScene.Instantiate() as Control;
            row.GetNode<Label>("LabelRank").Text = toOrdinal(i + 1);
            row.GetNode<Label>("LabelName").Text = orderedHighscores[i].Name;
            row.GetNode<Label>("LabelScore").Text = orderedHighscores[i].Score.ToString();
            row.GetNode<Label>("LabelTime").Text = $"{formatTime(orderedHighscores[i].Time)}";
            row.GetNode<Label>("LabelCollectedTreats").Text = orderedHighscores[i].CollectedTreats.ToString();

            displaySpecialTreats(row, orderedHighscores[i].SpecialTreats);
            container.AddChild(row);
        }
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public void OnButtonResetHighScoresPressed()
    {
        soundManager.PlaySoundClick();
        global.SaveData.ResetHighScores();
        ClearHighScores();
        ShowHighScores();
        resetHighScoreButton.ButtonPressed = false;
        resetHighScoreButton.FocusMode = FocusModeEnum.None;
    }

    private void displaySpecialTreats(Control row, string[] specialTreats)
    {
        Panel panelSpecialTreats = row.GetNode<Panel>("PanelSpecialTreats");

        TextureRect textureMeat = panelSpecialTreats.GetNode<TextureRect>("TextureMeat");
        TextureRect textureCheese = panelSpecialTreats.GetNode<TextureRect>("TextureCheese");
        TextureRect textureCookie = panelSpecialTreats.GetNode<TextureRect>("TextureCookie");

        if (specialTreats.Contains(TreatTypes.Meat.ToString()))
        {
            textureMeat.ShowBehindParent = false;
        }

        if (specialTreats.Contains(TreatTypes.Cheese.ToString()))
        {
            textureCheese.ShowBehindParent = false;
        }

        if (specialTreats.Contains(TreatTypes.Cookie.ToString()))
        {
            textureCookie.ShowBehindParent = false;
        }
    }
}
