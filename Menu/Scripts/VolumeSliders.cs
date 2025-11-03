using Godot;
using System;

public partial class VolumeSliders : VBoxContainer
{
    HSlider sliderMaster;
    HSlider sliderMusic;
    HSlider sliderSFX;

    Label labelMasterVolume;
    Label labelMusicVolume;
    Label labelSFXVolume;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        sliderMaster = GetNode<HSlider>("LabelMaster/SliderMaster");
        sliderMusic = GetNode<HSlider>("LabelMusic/SliderMusic");
        sliderSFX = GetNode<HSlider>("LabelSFX/SliderSFX");

        labelMasterVolume = GetNode<Label>("LabelMaster/SliderMaster/LabelMasterVolume");
        labelMusicVolume = GetNode<Label>("LabelMusic/SliderMusic/LabelMusicVolume");
        labelSFXVolume = GetNode<Label>("LabelSFX/SliderSFX/LabelSFXVolume");
    }

    public void OnMasterValueChanged(float value)
    {
        labelMasterVolume.Text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    public void OnMusicValueChanged(float value)
    {
        labelMusicVolume.Text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    public void OnSFXValueChanged(float value)
    {
        labelSFXVolume.Text = $"{Mathf.RoundToInt(value * 100f)}%";
    }
}
