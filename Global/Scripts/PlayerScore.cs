using Godot;
using System;

public partial class PlayerScore : Resource
{
    [Export]
    public string Name = "";
    [Export]
    public string Level = "";
    [Export]
    public int Score;
    [Export]
    public double Time;
    [Export]
    public int CollectedTreats;
    [Export]
    public string[] SpecialTreats = Array.Empty<string>();
}
