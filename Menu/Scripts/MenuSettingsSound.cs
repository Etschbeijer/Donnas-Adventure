using Godot;
using System;
using System.Threading.Tasks;

public partial class MenuSettingsSound : Control
{
    Global global;
    SoundManager soundManager;
    TransitionScreen transitionScreen;

    HSlider sliderSFX;
    HSlider sliderMusic;
    HSlider sliderMaster;

    Button menuButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Get nodes
        global = GetNode<Global>("/root/Global");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        transitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        sliderSFX = GetNode<HSlider>("VolumeSliders/LabelSFX/SliderSFX");
        sliderMusic = GetNode<HSlider>("VolumeSliders/LabelMusic/SliderMusic");
        sliderMaster = GetNode<HSlider>("VolumeSliders/LabelMaster/SliderMaster");

        menuButton = GetNode<VBoxContainer>("Buttons").GetNode<Button>("ButtonMenu");

        //Call important methods
        var masterLinearValue = soundManager.GetLinearValue("Master");
        sliderMaster.Value = masterLinearValue;
        var musicLinearValue = soundManager.GetLinearValue("Music");
        sliderMusic.Value = musicLinearValue;
        var sfxLinearValue = soundManager.GetLinearValue("SFX");
        sliderSFX.Value = sfxLinearValue;

        //Call important methods
        transitionScreen.FadeToNormal();

        if (global.IsControllerInput)
        {
            menuButton.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        var isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        UiAdapter.SetMouseUi(this, global, isControllerInput, menuButton);
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
}
