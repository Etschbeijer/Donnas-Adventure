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
        clearHighScores();
        showHighScores();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void clearHighScores()
    {
        foreach (Node child in container.GetChildren())
        {
            child.QueueFree();
        }
    }

    public void showHighScores()
    {
        var highScores = global.saveData.highscores;
        
        for (int i = 0; i < highScores.Length; i++)
        {
            var row = PanelRowScene.Instantiate() as Control;
            row.GetNode<Label>("LabelRank").Text = $"{i + 1}";
            row.GetNode<Label>("LabelName").Text = global.saveData.topPlayers[i];
            row.GetNode<Label>("LabelTime").Text = $"{formatTime(highScores[i])}";

            container.AddChild(row);
        }
    }

    private string formatTime(double time)
    {
        int minutes = (int) time / 60;
        int seconds = (int) time % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }

    public void onButtonMenuPressed()
    {
        soundManager.playSoundClick();
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }
}
