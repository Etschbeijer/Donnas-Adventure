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

    // Scores with no level or with a level that has no board (saved before level 2 existed, or from the test level)
    // show on the level 1 board, so its Reset button can clear them.
    public static string BoardIdOf(PlayerScore score)
    {
        return LevelCatalog.HasBoard(score.Level) ? score.Level : LevelCatalog.Level1Id;
    }

    public List<PlayerScore> GetBoard(string levelId)
    {
        return (PlayerScores ?? new()).Where(score => score != null && BoardIdOf(score) == levelId).OrderByDescending(score => score.Score).Take(ScoresPerBoard).ToList();
    }

    public void SetBoard(string levelId, IEnumerable<PlayerScore> scores)
    {
        var board = scores.Where(score => score != null).OrderByDescending(score => score.Score).Take(ScoresPerBoard);
        var others = (PlayerScores ?? new()).Where(score => score != null && BoardIdOf(score) != levelId);
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
