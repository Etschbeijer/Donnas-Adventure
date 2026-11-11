using Godot;
using System;
using System.Linq;

public partial class MenuWin : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    Label timescore;
    Label playerscore;

    Panel panelScore;

    Button buttonAccept;

    TextEdit textEditName;

    PlayerScore currentPlayer;

    Button playAgainButton;

    VirtualKeyboard virtualKeyboard;

    string boardId;
    bool canSaveScore;

    double currentTime;

    int score;
    int collectedTreats;

    string[] specialStreats;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        panelScore = GetNode<Panel>("PanelScore");
        timescore = panelScore.GetNode<Label>("LabelTimeScore");
        playerscore = panelScore.GetNode<Label>("LabelScoreValue");
        buttonAccept = panelScore.GetNode<Button>("ButtonAccept");
        textEditName = panelScore.GetNode<TextEdit>("TextEditName");
        virtualKeyboard = panelScore.GetNode<VirtualKeyboard>("VirtualKeyboard");
        playAgainButton = GetNode<VBoxContainer>("Buttons").GetNode<Button>("ButtonPlayAgain");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Visible;

        //Call important methods
        soundManager.StopMusicPuppyPlayingInTheGarden2D();
        soundManager.PlayMusicProject2Marioish2D();
        panelScore.Hide();
        SetHighscore();
        transitionScreen.FadeToNormal();
        virtualKeyboard.KeyButtonPressed += OnKeyButtonPressed;
        virtualKeyboard.DeleteButtonPressed += OnDeleteButtonPressed;

        if (global.IsControllerInput)
        {
            playAgainButton.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        if (!isControllerInput && isControllerInput != global.IsControllerInput)
        {
            virtualKeyboard.Hide();
        }

        UiAdapter.SetMouseUi(this, global, isControllerInput, playAgainButton);
    }

    public void OnKeyButtonPressed(string key)
    {
        textEditName.Text += key;
    }

    public void OnDeleteButtonPressed()
    {
        if (textEditName.Text.Length > 0)
        {
            textEditName.Text = textEditName.Text.Substring(0, textEditName.Text.Length - 1);
        }
    }

    public void OnTextEditNameFocusEntered()
    {
        if (virtualKeyboard != null && global.IsControllerInput)
        {
            virtualKeyboard.Show();
        }
    }

    public async void OnButtonPlayAgainPressed()
    {
        global.ResetSpecialTreats();
        global.ResetLevelId();
        await soundManager.PlaySoundPlay();
        transitionScreen.FadeToBlack();
        GetTree().ChangeSceneToFile(global.GetReplayScenePath());
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

        boardId = string.IsNullOrEmpty(global.LevelId) ? LevelCatalog.Level1Id : global.LevelId;
        canSaveScore = LevelCatalog.HasBoard(boardId);
        var board = global.SaveData.GetBoard(boardId);

        collectedTreats = global.CollectedTreats;
        specialStreats = global.SpecialTreats.ToArray();
        currentTime = (new TimeSpan(0, global.Minutes, global.Seconds)).TotalSeconds;
        score = HighscoreManager.CalculateScore((float)currentTime, collectedTreats, specialStreats.Length, global.ExpectedTime, global.ExpectedTreats);

        playerscore.Text = score.ToString();

        if (canSaveScore && (board.Count < SaveData.ScoresPerBoard || score > board[board.Count - 1].Score))
        {
            panelScore.Show();
        }
    }

    public void OnButtonAcceptPressed()
    {
        if (!canSaveScore)
        {
            return;
        }

        string playerName = textEditName.Text.Trim();

        currentPlayer = new PlayerScore
        {
            Name = playerName,
            Level = boardId,
            Score = score,
            Time = currentTime,
            CollectedTreats = collectedTreats,
            SpecialTreats = specialStreats
        };

        var newBoard = global.SaveData.GetBoard(boardId);
        newBoard.Add(currentPlayer);
        global.SaveData.SetBoard(boardId, newBoard);

        global.SaveData.Save();

        global.ResetSpecialTreats();
        global.ResetLevelId();

        panelScore.Hide();
    }
}
