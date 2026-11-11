using Godot;

public partial class SpeedPad : Area3D
{
    [Export]
    public float BoostSpeed = 13f;
    [Export]
    public float BoostDuration = 1.5f;

    public void OnBodyEntered(Node3D body)
    {
        if (body is PlayerCharacter player)
        {
            player.TryStartSpeedBoost(BoostSpeed, BoostDuration, -GlobalTransform.Basis.Z);
        }
    }
}
