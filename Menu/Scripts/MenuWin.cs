using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MenuWin : Control
{
    Global global;

    SoundManager soundManager;

    Label timescore;

    AcceptDialog nameDialog;

    double currentScore;
    List<(double, string)> highscoresAndPlayers = new List<(double, string)>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        global = GetNode<Global>("/root/Global");
        timescore = GetNode<Label>("LabelTimeScore");
        nameDialog = GetNode<AcceptDialog>("NameDialog");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        soundManager.stopMusicPuppyPlayingInTheGarden3D();
        soundManager.playMusicProject2Marioish2D();
        nameDialog.Hide();
        SetHighscore();
    }

    public async void onButtonPlayAgainPressed()
    {
        await soundManager.playSoundPlay();
        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public void onButtonMenuPressed()
    {
        soundManager.playSoundClick();
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void onButtonExitPressed()
    {
        await soundManager.playSoundExit();
        GetTree().Quit();
    }

    public void SetHighscore()
    {
        timescore.Text = $"{global.minutes:D2}:{global.seconds:D2}";

        var highscores = global.saveData.highscores?.ToList() ?? new List<double>();

        currentScore = (new TimeSpan(0, global.minutes, global.seconds)).TotalSeconds;

        bool addNewTime = highscores.Any(highscore => highscore > currentScore) || highscores.Count < 10;

        if (addNewTime)
        {
            var temp = global.saveData.topPlayers?.ToList() ?? new List<string>();

            for(int i = 0; i < temp.Count; i++)
            {
                highscoresAndPlayers.Add((highscores[i], temp[i]));
            }
            nameDialog.Show();
        }
    }

    public void onNameDialogConfirmed()
    {
        string playerName = nameDialog.GetNode<LineEdit>("LineEdit").Text.Trim();
        highscoresAndPlayers.Add((currentScore, playerName));
        highscoresAndPlayers.Sort ((rankA, rankB) => rankA.CompareTo(rankB));

        var top10ScoresAndPlayer = highscoresAndPlayers.Take(10).ToList();

        var topHighscores = new List<double>();
        var notIndexedTopPlayers = new List<string>();

        for (int i = 0; i < top10ScoresAndPlayer.Count; i++)
        {
            topHighscores.Add(highscoresAndPlayers[i].Item1);
            notIndexedTopPlayers.Add(highscoresAndPlayers[i].Item2);
        }

        global.saveData.highscores = topHighscores.ToArray();
        global.saveData.topPlayers = notIndexedTopPlayers.ToArray();

        global.saveData.Save();
    }
}
