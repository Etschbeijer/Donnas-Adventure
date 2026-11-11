using Godot;

public partial class BouncePad : Area3D
{
    [Export]
    public float LaunchVelocity = 13f;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AudioStreamPlayer3D audioPlayer;

    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer3D>("SoundBounce");
    }

    public void OnBodyEntered(Node3D body)
    {
        if (body is PlayerCharacter player && player.TryBounce(LaunchVelocity))
        {
            audioPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f);
            audioPlayer.Play();
        }
    }
}
