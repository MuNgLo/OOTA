using Godot;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Resources;
using System;

namespace OOTA.GameLogic;

public partial class GhostBuilding : MeshInstance3D
{
    [Export] Placer placer;
    PlayerAvatar Avatar => Core.Players.LocalPlayer.Avatar;
    GridLocation Location => placer.gridLocation;
    public override void _Ready()
    {
        LocalLogic.OnPlayerStateChanged += WhenPlayerStateChanged;
        LocalLogic.OnPlayerModeChanged += WhenPlayerModeChanged;
        HideGhost();
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private void WhenPlayerModeChanged(object sender, PLAYERMODE newMode)
    {
        if (ProcessMode == ProcessModeEnum.Disabled && newMode == PLAYERMODE.BUILDING)
        {
            ProcessMode = ProcessModeEnum.Inherit;
            return;
        }
        HideGhost();
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Location is null || Location.Coord == Core.Grid.WorldToCoord(Avatar.GlobalPosition))
        {
            HideGhost();
            return;
        }
        // If it is other team we stop all
        if (Location.Team != TEAM.NONE && Location.Team != Core.Players.LocalPlayer.Team)
        {
            HideGhost();
            return;
        }
        TowerResource tw = Core.Rules.towers.GetTowerByIndex(Core.Players.LocalPlayer.TowerIDX);
        if (!Core.Players.LocalPlayer.CanPay(tw.cost))
        {
            HideGhost();
            return;
        }
        //GD.Print($"[{Multiplayer.GetUniqueId()}]Placer::_PhysicsProcess() gridLocation[{gridLocation.Coord}].CanFit[{gridLocation.CanFit(tw)}] isBlocked[{isBlocked}]");
        if (Location is null || !Location.CanFit(tw))
        {
            HideGhost();
        }
        else
        {

            if (placer.IsBlocked)
            {
                HideGhost();
            }
            else
            {
                ShowPlacement(tw, Location);
                if (Input.IsActionPressed("Place"))
                {
                    Core.Rules.RequestPlaceTower(Core.Players.LocalPlayer.TowerIDX, Location.Coord);
                }
            }
        }
    }

    private void ShowPlacement(TowerResource tw, GridLocation gridLocation)
    {
        if (gridLocation is null) { return; }
        if (gridLocation.Foundation is not null)
        {
            GlobalPosition = gridLocation.Foundation.GlobalPosition + Vector3.Up * 0.669f;
        }
        else
        {
            GlobalPosition = Core.Grid.CoordToWorld(gridLocation.Coord);
        }
        Mesh = tw.mesh;
        Show();
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        ResetPhysicsInterpolation();
    }

    private void WhenPlayerStateChanged(object sender, PLAYERSTATE e)
    {
        if (e != PLAYERSTATE.DEAD)
        {
            HideGhost();
        }
    }
    private void HideGhost()
    {
        Hide();
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
    }
}// EOF CLASS
