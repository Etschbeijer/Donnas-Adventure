using Godot;
using System;

public partial class Global : Node
{
    public int CollectedTreats = 0;
    public int NumberOfTreats = 0;
    public int Minutes = 0;
    public int Seconds = 0;

    public bool CheckTime;

    public ulong StartTime;

    public SaveData SaveData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Call important methods
        SaveData = SaveData.LoadOrCreate();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (CheckTime)
        {
            ulong now = Time.GetTicksMsec();
            ulong elapsed = (now - StartTime) / 1000;

            Minutes = (int)(elapsed / 60);
            Seconds = (int)(elapsed % 60);
        }
    }

    public void IncreaseTreats()
    {
        CollectedTreats += 1;
    }

    public void DecreaseTreats()
    {
        CollectedTreats -= 1;
    }

    public void SetCurrentTreats(int numberOfTreats)
    {
        CollectedTreats = numberOfTreats;
    }

    public void SetNumberOfTreats(int countedTreats)
    {
        NumberOfTreats = countedTreats;
    }

    public void StartTimer()
    {
        CheckTime = true;
        StartTime = Time.GetTicksMsec();
    }

    public void StopTimer() 
    { 
        CheckTime = false; 
    }

    public void ResetTimer()
    {
        StopTimer();
        Minutes = 0;
        Seconds = 0;
    }
}
