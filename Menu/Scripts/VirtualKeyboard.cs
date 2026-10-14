using Godot;

public partial class VirtualKeyboard : Control
{
    Global global;

    Panel panel;

    Button shiftButton;
    Button deleteButton;

    GridContainer gridContainer;
    GridContainer gridContainer2;

    bool shift = false;
    bool symbols = false;

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
        gridContainer = panel.GetNode<GridContainer>("GridContainer");
        gridContainer2 = panel.GetNode<GridContainer>("GridContainer2");

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

    public void OnSpaceBarPressed()
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

    public void OnSmallThanPressed()
    {
        EmitSignal(SignalName.KeyButtonPressed, "<");
    }

    public void OnBiggerThanPressed()
    {
        EmitSignal(SignalName.KeyButtonPressed, ">");
    }

    public void OnSymbolsPressed()
    {
        symbols = !symbols;
        if (symbols)
        {
            gridContainer2.Show();
            gridContainer.Hide();
        }
        else
        {
            gridContainer.Show();
            gridContainer2.Hide();
        }
    }
}
