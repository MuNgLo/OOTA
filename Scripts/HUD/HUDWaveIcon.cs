using Godot;
using System;

namespace OOTA.HUD;
public partial class HUDWaveIcon : PanelContainer
{
    Label trackedDash;

    public override void _Process(double delta)
    {
        GlobalPosition = trackedDash.GlobalPosition;
    }

    public void SetDashToTrack(Label dash)
    {
        trackedDash = dash;
    }
}// EOF CLASS
