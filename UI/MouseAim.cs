using Godot;
using System;

public class MouseAim : CheckButton
{
    public override void _Toggled(bool buttonPressed)
    {
        base._Toggled(buttonPressed);
        Globals.MouseAim = buttonPressed;
    }
}
