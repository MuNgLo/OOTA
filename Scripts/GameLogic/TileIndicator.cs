using Godot;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Resources;
using System;
using System.Collections.Generic;

namespace OOTA.GameLogic;

public partial class TileIndicator : Node3D
{
    [Export] Placer placer;
    PlayerAvatar Avatar => Core.Players.LocalPlayer.Avatar;
    GridLocation Location => placer.gridLocation;

    public override void _Ready()
    {
        LocalLogic.OnPlayerStateChanged += WhenPlayerStateChanged;
        LocalLogic.OnPlayerModeChanged += WhenPlayerModeChanged;
        HideIndicator();
        ProcessMode = ProcessModeEnum.Disabled;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (Location is null || Location.Coord == Core.Grid.WorldToCoord(Avatar.GlobalPosition))
        {
            HideIndicator();
            return;
        }
        // If it is other team we stop all
        if (Location.Team != TEAM.NONE && Location.Team != Core.Players.LocalPlayer.Team)
        {
            HideIndicator();
            return;
        }

        GlobalPosition = Core.Grid.CoordToWorld(Location.Coord);
        if (!Visible)
        {
            ShowIndicator();
        }

        TowerResource tw = Core.Rules.towers.GetTowerByIndex(Core.Players.LocalPlayer.TowerIDX);

        if (Visible && !Location.CanFit(tw))
        {
            List<PlayerActionStruct> interactions = Location.GetInteractions();
            if (interactions.Count > 0 && Input.IsActionJustPressed("Place"))
            {
                LocalLogic.RaiseHudInteractMenu(this, interactions);
            }
        }
    }


    private void WhenPlayerModeChanged(object sender, PLAYERMODE newMode)
    {
        if (newMode == PLAYERMODE.BUILDING)
        {
            ProcessMode = ProcessModeEnum.Inherit;
            return;
        }
        HideIndicator();
        ProcessMode = ProcessModeEnum.Disabled;
    }
    private void WhenPlayerStateChanged(object sender, PLAYERSTATE e)
    {
        if (e == PLAYERSTATE.DEAD)
        {
            HideIndicator();
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private void HideIndicator()
    {
        Hide();
        PhysicsInterpolationMode = MeshInstance3D.PhysicsInterpolationModeEnum.Off;
    }

    private void ShowIndicator()
    {
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Inherit;
        ResetPhysicsInterpolation();
        Show();
    }
}// EOF CLASS
