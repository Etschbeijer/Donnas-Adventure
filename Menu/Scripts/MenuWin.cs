using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

public partial class MenuWin : Control
{
    Global global;

    SoundManager soundManager;

    Label timescore;

    AcceptDialog nameDialog;

    TransitionScreen transitionScreen;

    double currentTime;

    int score;
    int collectedTreats;

    string[] specialStreats;

    List<PlayerScore> highscoresAndPlayers = new();

    PlayerScore currentPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        timescore = GetNode<Label>("LabelTimeScore");
        nameDialog = GetNode<AcceptDialog>("NameDialog");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        soundManager.StopMusicPuppyPlayingInTheGarden2D();
        soundManager.PlayMusicProject2Marioish2D();
        nameDialog.Hide();
        SetHighscore();
        transitionScreen.FadeToNormal();
    }

    public async void OnButtonPlayAgainPressed()
    {
        global.ResetSpecialTreats();
        global.ResetLevelId();
        await soundManager.PlaySoundPlay();
        transitionScreen.FadeToBlack();
        GetTree().ChangeSceneToFile("res://Levels/Nodes/level_1.tscn");
    }

    public async void OnButtonMenuPressed()
    {
        global.ResetSpecialTreats();
        global.ResetLevelId();
        soundManager.PlaySoundClick();
        transitionScreen.FadeToBlack();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        global.ResetSpecialTreats();
        global.ResetLevelId();

        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        await ToSignal(transitionScreen.AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().Quit();
    }

    public void SetHighscore()
    {
        timescore.Text = $"{global.Minutes:D2}:{global.Seconds:D2}";

        var playerScores = global.SaveData.PlayerScores?.ToList() ?? new();

        collectedTreats = global.CollectedTreats;
        specialStreats = global.SpecialTreats.ToArray();
        currentTime = (new TimeSpan(0, global.Minutes, global.Seconds)).TotalSeconds;
        score = HighscoreManager.CalculateScore((float)currentTime, collectedTreats, specialStreats.Length);

        bool addNewTime = playerScores.Any(highscore => score > highscore.Score) || playerScores.Count < 10;

        if (addNewTime)
        {
            for(int i = 0; i < playerScores.Count; i++)
            {
                highscoresAndPlayers.Add(playerScores[i]);
            }
            nameDialog.Show();
        }
    }

    public void OnNameDialogConfirmed()
    {
        string playerName = nameDialog.GetNode<LineEdit>("LineEdit").Text.Trim();

        currentPlayer = new PlayerScore
        {
            Name = playerName,
            Level = global.LevelId,
            Score = score,
            Time = currentTime,
            CollectedTreats = collectedTreats,
            SpecialTreats = specialStreats
        };

        currentPlayer.Name = playerName;
        highscoresAndPlayers.Add(currentPlayer);
        highscoresAndPlayers.Sort ((rankA, rankB) => rankA.Score.CompareTo(rankB.Score));

        var top10ScoresAndPlayer = highscoresAndPlayers.Take(10).ToList();

        var topPlayers = new List<PlayerScore>();

        for (int i = 0; i < top10ScoresAndPlayer.Count; i++)
        {
            topPlayers.Add(highscoresAndPlayers[i]);
        }

        topPlayers = topPlayers.OrderByDescending(playerScore => playerScore.Score).ToList();

        global.SaveData.PlayerScores = new(topPlayers);

        global.SaveData.Save();

        global.ResetSpecialTreats();
        global.ResetLevelId();
    }
}
