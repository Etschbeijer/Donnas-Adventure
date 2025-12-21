using Godot;
using static Godot.Control;

public static class UiAdapter
{
    public static void SetMouseUi(Control owner, Global global, bool isControllerInput, Button button)
    {
        if (!isControllerInput && isControllerInput != global.IsControllerInput)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;

            owner.GetViewport().GuiReleaseFocus();
        }

        if (isControllerInput && isControllerInput != global.IsControllerInput)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;

            owner.GetViewport().GuiReleaseFocus();
            ResetGuiHover(owner.GetTree().Root);

            button.GrabFocus();
        }
        global.IsControllerInput = isControllerInput;
    }

    public static void ResetGuiHover(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Control c)
            {
                GD.Print($"c: {c.Name}");
                c.MouseFilter = MouseFilterEnum.Ignore;
                c.MouseFilter = MouseFilterEnum.Stop;

                ResetGuiHover(c);
            }
        }
    }
}
