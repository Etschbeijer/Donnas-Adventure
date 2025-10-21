using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuSettingsSound : Control
{
    SoundManager soundManager;

    HSlider sliderMaster;
    HSlider sliderMusic;
    HSlider sliderSFX;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        sliderMaster = GetNode<HSlider>("VolumeSliders/LabelMaster/SliderMaster");
        sliderMusic = GetNode<HSlider>("VolumeSliders/LabelMusic/SliderMusic");
        sliderSFX = GetNode<HSlider>("VolumeSliders/LabelSFX/SliderSFX");

        //Call important methods
        var masterLinearValue = soundManager.GetLinearValue("Master");
        sliderMaster.Value = masterLinearValue;
        var musicLinearValue = soundManager.GetLinearValue("Music");
        sliderMusic.Value = musicLinearValue;
        var sfxLinearValue = soundManager.GetLinearValue("SFX");
        sliderSFX.Value = sfxLinearValue;
    }

    public void OnMasterValueChanged(float value)
    {
        soundManager.SetVolume("Master", value);
    }

    public void OnMusicValueChanged(float value)
    {
        soundManager.SetVolume("Music", value);
    }

    public void OnSFXValueChanged(float value)
    {
        soundManager.SetVolume("SFX", value);
    }

    public void OnButtonMenuPressed()
    {
        soundManager.PlaySoundMainMenu();
        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        await soundManager.PlaySoundExit();

        GetTree().Quit();
    }
}
