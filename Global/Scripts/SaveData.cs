using Godot;
using System;
using System.Collections.Generic;

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
