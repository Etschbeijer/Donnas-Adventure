using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SaveData : Resource
{
    [Export]
    public Godot.Collections.Array<PlayerScore> PlayerScores = new();

    const string savePathHighScores = "user://highscores.tres";

    public static SaveData LoadOrCreate()
    {
        SaveData res;

        if (FileAccess.FileExists(savePathHighScores))
        {
            res = (SaveData)ResourceLoader.Load(savePathHighScores).Duplicate() as SaveData;
        }
        else
        {
            res = new SaveData();
        }
        
        return res;
    }

    public void ResetHighScores()
    {
        PlayerScores = new ();
        Save();
        LoadOrCreate();
    }

    public const int ScoresPerBoard = 10;

    // Scores saved before level 2 existed have no level and belong to level 1.
    public static string BoardIdOf(PlayerScore score)
    {
        return string.IsNullOrEmpty(score.Level) ? LevelCatalog.Level1Id : score.Level;
    }

    public List<PlayerScore> GetBoard(string levelId)
    {
        return PlayerScores.Where(score => BoardIdOf(score) == levelId).OrderByDescending(score => score.Score).ToList();
    }

    public void SetBoard(string levelId, IEnumerable<PlayerScore> scores)
    {
        var board = scores.OrderByDescending(score => score.Score).Take(ScoresPerBoard);
        var others = PlayerScores.Where(score => BoardIdOf(score) != levelId);
        PlayerScores = new(others.Concat(board));
    }

    public void ResetHighScores(string levelId)
    {
        SetBoard(levelId, new List<PlayerScore>());
        Save();
    }

    public void Reset()
    {
        SaveData res;

        if (FileAccess.FileExists(savePathHighScores))
        {
            res = new SaveData();
            Save();
            LoadOrCreate();
        }
    }

    public void Save()
    {
        var dir = savePathHighScores.GetBaseDir();
        DirAccess.MakeDirRecursiveAbsolute(dir);
        
        var err = ResourceSaver.Save(this, savePathHighScores);
        if (err == Error.Ok)
        {
            GD.Print($"Highscores have been saved successful: {err}");
        }
        else
        {
            GD.Print($"Failed to save highscores: {err}");
        }
    }
}
