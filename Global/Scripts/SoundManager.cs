using Godot;
using System;
using System.Threading.Tasks;

public partial class SoundManager : Control
{
    private string settingsPath = "user://settings.cfg";

    private ConfigFile configFile = new ConfigFile();

    RandomNumberGenerator rng = new RandomNumberGenerator();

    public AudioStreamPlayer2D SoundLose;
    public AudioStreamPlayer2D SoundWin;
    public AudioStreamPlayer2D SoundClick;
    public AudioStreamPlayer2D SoundExit;
    public AudioStreamPlayer2D SoundMainMenu;
    public AudioStreamPlayer2D SoundStartGame;

    public AudioStreamPlayer2D MusicProject2Marioish2D;
    public AudioStreamPlayer2D MusicPuppyPlayingInTheGarden2D;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        SoundLose = GetNode<AudioStreamPlayer2D>("SoundLose");
        SoundWin = GetNode<AudioStreamPlayer2D>("SoundWin");
        SoundClick = GetNode<AudioStreamPlayer2D>("SoundClick");
        SoundExit = GetNode<AudioStreamPlayer2D>("SoundExit");
        SoundMainMenu = GetNode<AudioStreamPlayer2D>("SoundMainMenu");
        SoundStartGame = GetNode<AudioStreamPlayer2D>("SoundPlay");

        MusicProject2Marioish2D = GetNode<AudioStreamPlayer2D>("MusicProject2Marioish2D");
        MusicPuppyPlayingInTheGarden2D = GetNode<AudioStreamPlayer2D>("MusicPuppyPlayingInTheGarden2D");

        //Call important methods
        LoadVolume();
        PlayMusicProject2Marioish2D();
    }

    public float GetLinearValue(string busName)
    {
        if (configFile.HasSectionKey("sound", busName))
        {
            return (float)configFile.GetValue("sound", busName);
        }
        else
        {
            throw new Exception($"The section \"sound\" could not be found.");
        }
    }

    public void SetVolume(string busName, float linearValue)
    {
        float dbValue = Mathf.LinearToDb(linearValue);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(busName), dbValue);
        configFile.SetValue("sound", busName, linearValue);
        configFile.Save(settingsPath);
    }

    public void LoadVolume()
    {
        if (configFile.Load(settingsPath) != Error.Ok)
        {
            var configFile = new ConfigFile();
            configFile.Save(settingsPath);
        }

        foreach (string busName in new string[] { "Master", "Music", "SFX" })
        {
            if (configFile.HasSectionKey("sound", busName))
            {
                float linearValue = (float)configFile.GetValue("sound", busName);
                float dbValue = Mathf.LinearToDb(linearValue);
                AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(busName), dbValue);
            }
            else
            {
                configFile.SetValue("sound", busName, 1f);
                AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(busName), 1f);
                configFile.Save(settingsPath);
            }
        }
    }

    void PlaySound(AudioStreamPlayer2D player)
    {
        player.PitchScale = rng.RandfRange(0.9f, 1.1f);
        player.Play();
    }

    async Task PlaySoundAndWait(AudioStreamPlayer2D player)
    {
        PlaySound(player);
        await ToSignal(player, AudioStreamPlayer2D.SignalName.Finished);
    }

    public void PlaySoundClick() => PlaySound(SoundClick);

    public void PlaySoundMainMenu() => PlaySound(SoundMainMenu);

    public Task PlaySoundExit() => PlaySoundAndWait(SoundExit);

    public Task PlaySoundPlay() => PlaySoundAndWait(SoundStartGame);

    public void EnsureMenuMusic()
    {
        if (IsMusicPuppyPlayingInTheGarden2DPlaying())
            StopMusicPuppyPlayingInTheGarden2D();

        if (!IsMusicProject2Marioish2DPlaying())
            PlayMusicProject2Marioish2D();
    }

    public void PlayMusicProject2Marioish2D() => MusicProject2Marioish2D.Play();

    public bool IsMusicProject2Marioish2DPlaying() => MusicProject2Marioish2D.Playing;

    public void StopMusicProject2Marioish2D() => MusicProject2Marioish2D.Stop();

    public void PlayMusicPuppyPlayingInTheGarden2D() => MusicPuppyPlayingInTheGarden2D.Play();

    public bool IsMusicPuppyPlayingInTheGarden2DPlaying() => MusicPuppyPlayingInTheGarden2D.Playing;

    public void StopMusicPuppyPlayingInTheGarden2D() => MusicPuppyPlayingInTheGarden2D.Stop();

    public void PlaySoundLose()
    {
        PlaySound(SoundLose);

        StopMusicPuppyPlayingInTheGarden2D();
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
