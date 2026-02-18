using Godot;
using System;

public partial class UIGameVersion : Label
{
    public override void _Ready()
    {
        Text = ProjectSettings.GetSetting("application/config/version").AsString();
    }
}
