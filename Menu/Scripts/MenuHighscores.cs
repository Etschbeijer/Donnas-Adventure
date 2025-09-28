using Godot;
using System;

public partial class MenuHighscores : Control
{
    [Export]
    PackedScene PanelRowScene;
    
    Global global;

    SoundManager soundManager;

    VBoxContainer container;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        container = GetNode<VBoxContainer>("VBoxContainer");

        //Call important methods
        ClearHighScores();
        ShowHighScores();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

    public void ShowHighScores()
    {
        var highScores = global.SaveData.highscores;
        
        for (int i = 0; i < highScores.Length; i++)
        {
            var row = PanelRowScene.Instantiate() as Control;
            row.GetNode<Label>("LabelRank").Text = $"{i + 1}";
            row.GetNode<Label>("LabelName").Text = global.SaveData.topPlayers[i];
            row.GetNode<Label>("LabelTime").Text = $"{formatTime(highScores[i])}";

            container.AddChild(row);
        }
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }
}
