using Godot;

public partial class VirtualKeyboard : Control
{
    Global global;

    Panel panel;

    Button playButton;

    //Define Signals
    [Signal]
    public delegate void KeyButtonPressedEventHandler(string key);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");

        foreach (Button keyButton in GetTree().GetNodesInGroup("KeyButtons"))
        {
            string key = keyButton.Text;
            keyButton.Pressed += () => EmitSignal(SignalName.KeyButtonPressed, key);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
