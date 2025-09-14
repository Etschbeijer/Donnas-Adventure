using Godot;
using System;
using System.Threading.Tasks;

public partial class SoundManager : Control
{
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public AudioStreamPlayer2D soundClick;
    public AudioStreamPlayer2D soundExit;
    public AudioStreamPlayer2D soundMainMenu;
    public AudioStreamPlayer2D sounPlay;

    public AudioStreamPlayer2D musicProject2Marioish2D;
    public AudioStreamPlayer2D musicPuppyPlayingInTheGarden2D;

    public AudioStreamPlayer3D soundLose;
    public AudioStreamPlayer3D soundWin;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        soundClick = GetNode<AudioStreamPlayer2D>("SoundClick");
        soundExit = GetNode<AudioStreamPlayer2D>("SoundExit");
        soundMainMenu = GetNode<AudioStreamPlayer2D>("SoundMainMenu");
        sounPlay = GetNode<AudioStreamPlayer2D>("SoundPlay");

        musicProject2Marioish2D = GetNode<AudioStreamPlayer2D>("MusicProject2Marioish2D");
        musicPuppyPlayingInTheGarden2D = GetNode<AudioStreamPlayer2D>("MusicPuppyPlayingInTheGarden2D");

        soundLose = GetNode<AudioStreamPlayer3D>("SoundLose");
        soundWin = GetNode<AudioStreamPlayer3D>("SoundWin");

        //Call important methods
        playMusicProject2Marioish2D();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void playSoundClick()
    {
        soundClick.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundClick.Play();
    }

    public async Task playSoundExit()
    {
        soundExit.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundExit.Play();

        await ToSignal(soundExit, AudioStreamPlayer2D.SignalName.Finished);
    }

    public void playSoundMainMenu()
    {
        soundMainMenu.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundMainMenu.Play();
    }

    public void playMusicProject2Marioish2D()
    {
        musicProject2Marioish2D.Play();
    }

    public void stopMusicProject2Marioish2D()
    {
        musicProject2Marioish2D.Stop();
    }

    public void playMusicPuppyPlayingInTheGarden3D()
    {
        musicPuppyPlayingInTheGarden2D.Play();
    }

    public void stopMusicPuppyPlayingInTheGarden3D()
    {
        musicPuppyPlayingInTheGarden2D.Stop();
    }

    public async Task playSoundPlay()
    {
        sounPlay.PitchScale = rng.RandfRange(0.9f, 1.1f);
        sounPlay.Play();

        await ToSignal(sounPlay, AudioStreamPlayer2D.SignalName.Finished);
    }

    public void playSoundLose()
    {
        soundLose.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundLose.Play();

        stopMusicPuppyPlayingInTheGarden3D();
    }

    public void playSoundWin()
    {
        soundWin.PitchScale = rng.RandfRange(0.9f, 1.1f);
        var defaultDB = soundWin.VolumeDb;
        soundWin.VolumeDb = soundWin.VolumeDb * 20f;
        soundWin.Play();
        soundWin.VolumeDb = defaultDB;
    }
}
