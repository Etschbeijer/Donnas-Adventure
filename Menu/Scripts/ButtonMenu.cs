using Godot;
using System;

public partial class ButtonMenu : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetViewport().GuiReleaseFocus();
        GrabFocus();
	}
}
