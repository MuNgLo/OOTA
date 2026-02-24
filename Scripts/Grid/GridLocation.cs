using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Resources;
using System;

namespace OOTA.Grid;


public class GridLocation
{
    Vector2I coord;
    BuildingBaseClass foundation;
    BuildingBaseClass tower;
    public bool IsFree => foundation is null && tower is null;

    public BuildingBaseClass Foundation { get => foundation; set { SetFoundation(value); } }

    private void SetFoundation(BuildingBaseClass value)
    {
        if (value is null && tower is not null)
        {
            Tower = null;
        }
        foundation = value;
    }

    public BuildingBaseClass Tower { get => tower; set { SetTower(value); } }

    private void SetTower(BuildingBaseClass value)
    {
        if (value is null && tower is not null)
        {
            tower.QueueFree();
        }
        tower = value;
    }

    public Vector2I Coord => coord;
    public TEAM Team => ResolveTeam();

    private TEAM ResolveTeam()
    {
        if (foundation is not null)
        {
            return foundation.Team;
        }
        if (tower is not null)
        {
            return tower.Team;
        }
        return TEAM.NONE;
    }

    internal void SetBuilding(BuildingBaseClass building)
    {
        if (building.towerType == TOWERTYPE.FOUNDATION) { Foundation = building; }
        else if (building.towerType == TOWERTYPE.ATTACK)
        {
            Tower = building;
            if (foundation is not null) { Tower.GlobalPosition += Vector3.Up * 0.669f; }
        }
    }

    internal bool CanFit(TowerResource tw)
    {
        if (foundation is not null)
        {
            if (tower is null && tw.towerType == TOWERTYPE.ATTACK)
            {
                return true;
            }
            return false;
        }
        if (tower is not null)
        {
            return false;
        }
        return true;
    }

    public GridLocation(Vector2I coord)
    {
        this.coord = coord;
    }
}// EOF CLASs
