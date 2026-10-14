using System.Diagnostics;

public partial class PlayerState : State
{
    public PlayerCharacter Player { get; set; }
    
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Player = Owner as PlayerCharacter;

        Debug.Assert(Player != null);
    }

}
