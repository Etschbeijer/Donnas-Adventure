public static class LevelCatalog
{
    public const string Level1 = "res://Levels/Nodes/level_1.tscn";
    public const string Level2 = "res://Levels/Nodes/level_2.tscn";
    public const string Level1Id = "1-1";
    public const string Level2Id = "1-2";

    public static readonly (string Id, string Name)[] HighscoreBoards = { (Level1Id, "Level 1"), (Level2Id, "Level 2") };
}
