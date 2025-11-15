using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuSettingsSound : Control
{
    TransitionScreen transitionScreen;

    SoundManager soundManager;

    HSlider sliderMaster;
    HSlider sliderMusic;
    HSlider sliderSFX;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

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

        transitionScreen.FadeToNormal();
    }

    public void OnButtonResetPressed()
    {
        sliderSFX.EmitSignal("value_changed", 1f);
        sliderSFX.Value = 1f;
        sliderMusic.EmitSignal("value_changed", 1f);
        sliderMusic.Value = 1f;
        sliderMaster.EmitSignal("value_changed", 1f);
        sliderMaster.Value = 1f;
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

        transitionScreen.FadeToBlack();

        GetTree().ChangeSceneToFile("res://Menu/Nodes/menu_title.tscn");
    }

    public async void OnButtonExitPressed()
    {
        transitionScreen.FadeToBlack();

        await soundManager.PlaySoundExit();

        GetTree().Quit();
    }
}
