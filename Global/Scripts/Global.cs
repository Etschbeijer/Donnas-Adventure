using Godot;
using System;

public partial class Global : Node
{
    public int collectedTreats = 0;
    public int numberOfTreats = 0;
    public int minutes = 0;
    public int seconds = 0;

    public bool checkTime;

    public ulong startTime;

    public SaveData saveData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Call important methods
        saveData = SaveData.LoadOrCreate();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (checkTime)
        {
            ulong now = Time.GetTicksMsec();
            ulong elapsed = (now - startTime) / 1000;

            minutes = (int)(elapsed / 60);
            seconds = (int)(elapsed % 60);
        }
    }

    public void increaseTreats()
    {
        collectedTreats += 1;
    }

    public void decreaseTreats()
    {
        collectedTreats -= 1;
    }

    public void setCurrentTreats(int numberOfTreats)
    {
        collectedTreats = numberOfTreats;
    }

    public void setNumberOfTreats(int countedTreats)
    {
        numberOfTreats = countedTreats;
    }

    public void startTimer()
    {
        checkTime = true;
        startTime = Time.GetTicksMsec();
    }

    public void stopTimer() 
    { 
        checkTime = false; 
    }

    public void resetTimer()
    {
        stopTimer();
        minutes = 0;
        seconds = 0;
    }
}
