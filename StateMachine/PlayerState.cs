using System;
using System.Diagnostics;
using Godot;

public partial class PlayerState : State
{
    public PlayerCharacter Player { get; set; }
    
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // Waiting for the owner to be ready
        WaitForOwnerReady();

        Player = Owner as PlayerCharacter;

        Debug.Assert(Player != null);
    }

    private async void WaitForOwnerReady()
    {
        // Waiting until the owner is ready
        await ToSignal(Owner, "ready");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
