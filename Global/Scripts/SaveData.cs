using Godot;
using System;
using System.Collections.Generic;

public partial class SaveData : Resource
{
    [Export]
    public double[] highscores = new double[0];
    [Export]
    public string[] topPlayers = new string[0];

    const string SavePath = "user://highscores.tres";

    public static SaveData LoadOrCreate()
    {
        SaveData res;

        if (FileAccess.FileExists(SavePath))
        {
            res = (SaveData)ResourceLoader.Load(SavePath).Duplicate() as SaveData;
        }
        else
        {
            res = new SaveData();
        }
        
        return res;
    }

    public void Save()
    {
        var dir = SavePath.GetBaseDir();
        DirAccess.MakeDirRecursiveAbsolute(dir);
        
        var err = ResourceSaver.Save(this, SavePath);
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
