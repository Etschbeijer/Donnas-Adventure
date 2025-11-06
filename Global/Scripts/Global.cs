using Godot;
using System;
using System.Linq;

public partial class Global : Node
{
    public int CollectedTreats = 0;
    public int NumberOfTreats = 0;
    public int Minutes = 0;
    public int Seconds = 0;

    public bool CheckTime = false;
    public bool PauseTime = false;

    public ulong StartTime = 0;
    public ulong PausedTime = 0;

    public SaveData SaveData;

    public Deathypes Deathypes;

    Node explosions;

    PackedScene grassExplosion;

    GpuParticles3D dustExplosion;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        //explosions = GetNode<Node3D>("Explosions");
        //dustExplosion = GD.Load<Node3D>("res://Effects/Nodes/earth_explosion.tscn").GetNode<GpuParticles3D>("Explosion");
        grassExplosion = (GD.Load<PackedScene>("res://Effects/Nodes/grass_explosion.tscn"));

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

    public void IncreaseTreats(int value)
    {
        CollectedTreats += value;
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
        PauseTime = false;
        if (StartTime == 0)
        {
            StartTime = Time.GetTicksMsec();
        }
        else
        {
            StartTime = Time.GetTicksMsec() - PausedTime;
        }
    }

    public void StopTimer() 
    { 
        CheckTime = false;
        PauseTime = true;
        PausedTime = Time.GetTicksMsec() - StartTime;
    }

    public void ResetTimer()
    {
        StopTimer();
        StartTime = 0;
        PausedTime = 0;
        Minutes = 0;
        Seconds = 0;
    }

    public void DustExplosion()
    {
        dustExplosion.Emitting = true;
    }

    public void FireExplosions(GpuParticles3D[] explosions)
    {
        foreach (GpuParticles3D explosion in explosions)
        {
            explosion.Restart();
            explosion.Emitting = true;
        }
    }

    public void CreateGrassExplosion(Vector3 position, float scaling)
    {
        if (grassExplosion != null)
        {
            var instance = (Node3D) grassExplosion.Instantiate();
            GetTree().CurrentScene.AddChild(instance);

            instance.Scale = new Vector3(scaling, scaling, scaling);
            instance.GlobalPosition = new Vector3(position.X, position.Y - 0.25f, position.Z); ;
            var explosions = instance.GetChildren().OfType<GpuParticles3D>().Where(explosion => explosion.Name.ToString().Contains("Explosion")).ToArray();
            FireExplosions(explosions);
        }
    }

    public void SetDeathType(Deathypes deathype)
    {
        Deathypes = deathype;
    }
}
