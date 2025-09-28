using Godot;
using System;
using System.Threading.Tasks;

public partial class SoundManager : Control
{
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public AudioStreamPlayer2D SoundClick;
    public AudioStreamPlayer2D SoundExit;
    public AudioStreamPlayer2D SoundMainMenu;
    public AudioStreamPlayer2D SoundStartGame;

    public AudioStreamPlayer2D MusicProject2Marioish2D;
    public AudioStreamPlayer2D MusicPuppyPlayingInTheGarden2D;

    public AudioStreamPlayer3D SoundLose;
    public AudioStreamPlayer3D SoundWin;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        SoundClick = GetNode<AudioStreamPlayer2D>("SoundClick");
        SoundExit = GetNode<AudioStreamPlayer2D>("SoundExit");
        SoundMainMenu = GetNode<AudioStreamPlayer2D>("SoundMainMenu");
        SoundStartGame = GetNode<AudioStreamPlayer2D>("SoundPlay");

        MusicProject2Marioish2D = GetNode<AudioStreamPlayer2D>("MusicProject2Marioish2D");
        MusicPuppyPlayingInTheGarden2D = GetNode<AudioStreamPlayer2D>("MusicPuppyPlayingInTheGarden2D");

        SoundLose = GetNode<AudioStreamPlayer3D>("SoundLose");
        SoundWin = GetNode<AudioStreamPlayer3D>("SoundWin");

        //Call important methods
        PlayMusicProject2Marioish2D();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void PlaySoundClick()
    {
        SoundClick.PitchScale = rng.RandfRange(0.9f, 1.1f);
        SoundClick.Play();
    }

    public async Task PlaySoundExit()
    {
        SoundExit.PitchScale = rng.RandfRange(0.9f, 1.1f);
        SoundExit.Play();

        await ToSignal(SoundExit, AudioStreamPlayer2D.SignalName.Finished);
    }

    public void PlaySoundMainMenu()
    {
        SoundMainMenu.PitchScale = rng.RandfRange(0.9f, 1.1f);
        SoundMainMenu.Play();
    }

    public void PlayMusicProject2Marioish2D()
    {
        MusicProject2Marioish2D.Play();
    }

    public void StopMusicProject2Marioish2D()
    {
        MusicProject2Marioish2D.Stop();
    }

    public void PlayMusicPuppyPlayingInTheGarden3D()
    {
        MusicPuppyPlayingInTheGarden2D.Play();
    }

    public void StopMusicPuppyPlayingInTheGarden3D()
    {
        MusicPuppyPlayingInTheGarden2D.Stop();
    }

    public async Task PlaySoundPlay()
    {
        SoundStartGame.PitchScale = rng.RandfRange(0.9f, 1.1f);
        SoundStartGame.Play();

        await ToSignal(SoundStartGame, AudioStreamPlayer2D.SignalName.Finished);
    }

    public void PlaySoundLose()
    {
        SoundLose.PitchScale = rng.RandfRange(0.9f, 1.1f);
        SoundLose.Play();

        StopMusicPuppyPlayingInTheGarden3D();
    }

    public void PlaySoundWin()
    {
        SoundWin.PitchScale = rng.RandfRange(0.9f, 1.1f);
        var defaultDB = SoundWin.VolumeDb;
        SoundWin.VolumeDb = SoundWin.VolumeDb * 20f;
        SoundWin.Play();
        SoundWin.VolumeDb = defaultDB;
    }
}
