using Godot;

public partial class VirtualKeyboard : Control
{
    Global global;

    Panel panel;

    Button shiftButton;
    Button deleteButton;

    bool shift = false;

    //Define Signals
    [Signal]
    public delegate void KeyButtonPressedEventHandler(string key);

    [Signal]
    public delegate void DeleteButtonPressedEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        global = GetNode<Global>("/root/Global");

        var panel = GetNode<Panel>("Panel");
        shiftButton = panel.GetNode<Button>("Shift");
        deleteButton = panel.GetNode<Button>("Delete");

        foreach (Button keyButton in GetTree().GetNodesInGroup("KeyButtons"))
        {
            keyButton.Pressed += () => OnKeyButtonPressed(keyButton);
        }

        deleteButton.Pressed += () => EmitSignal(SignalName.DeleteButtonPressed);
    }

    public void OnKeyButtonPressed(Button button)
    {
        EmitSignal(SignalName.KeyButtonPressed, button.Text);
    }

    public void OnBackspacePressed()
    {
        EmitSignal(SignalName.KeyButtonPressed, " ");
    }

    public void OnShiftPressed()
    {
        foreach (Button keyButton in GetTree().GetNodesInGroup("KeyButtons"))
        {
            string key = shift ? keyButton.Text.ToUpper() : keyButton.Text.ToLower();
            keyButton.Text = key;
        }
        shift = !shift;
    }
}
