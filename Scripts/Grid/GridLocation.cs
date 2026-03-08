using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Resources;
using System;
using System.Collections.Generic;

namespace OOTA.Grid;


public class GridLocation
{
    Vector2I coord;
    BuildingBaseClass foundation;
    BuildingBaseClass building;

    public bool canBuild = true;
    public bool IsFree => canBuild && foundation is null && building is null;
    public TEAM Team => ResolveTeam();
    public Vector2I Coord => coord;

    public BuildingBaseClass Foundation { get => foundation; set { SetFoundation(value); } }

    private void SetFoundation(BuildingBaseClass value)
    {
        if (value is null)
        {
            if (building is not null) { Building = null; }
        }
        if (foundation is not null) { foundation.QueueFree(); }
        foundation = value;
    }

    /// <summary>
    /// Setting Tower to Null will queueFree existing Tower
    /// </summary>
    public BuildingBaseClass Building { get => building; set { SetBuilding(value); } }

    private void SetBuilding(BuildingBaseClass newBuilding)
    {
        if (newBuilding is null)
        {
            if (building is not null) { building.QueueFree(); building = null; return; }
            if (foundation is not null) { Foundation = null; return; }
        }

        if (newBuilding.towerType == TOWERTYPE.FOUNDATION)
        {
            Foundation = newBuilding;
        }
        else
        {
            building = newBuilding;
            if (foundation is not null) { building.GlobalPosition += Vector3.Up * 0.669f; }
        }
    }

    private TEAM ResolveTeam()
    {
        if (foundation is not null)
        {
            return foundation.Team;
        }
        if (building is not null)
        {
            return building.Team;
        }
        return TEAM.NONE;
    }



    internal bool CanFit(TowerResource tw)
    {
        if (!canBuild) { return false; }
        if (foundation is not null)
        {
            if (building is null && tw.towerType == TOWERTYPE.ATTACK)
            {
                return true;
            }
            return false;
        }
        if (building is not null)
        {
            return false;
        }
        return true;
    }

    internal List<PlayerActionStruct> GetInteractions()
    {
        List<PlayerActionStruct> interactions = new List<PlayerActionStruct>();

        // Check repair option
        if (foundation is not null && foundation.Health < foundation.MaxHealth) { interactions.Add(PlayerActionStruct.Repair(coord, foundation.CurrentRepairCost)); }
        else if (building is not null && building.Health < building.MaxHealth) { interactions.Add(PlayerActionStruct.Repair(coord, building.CurrentRepairCost)); }

        // Check Sell option
        if (building is not null && building.canTakeDamage) { interactions.Add(PlayerActionStruct.Sell(coord, building.CurrentSellValue)); }
        else if (foundation is not null && foundation.canTakeDamage) { interactions.Add(PlayerActionStruct.Sell(coord, foundation.CurrentSellValue)); }

        // Add building's interactions
        if(building is not null)
        {
            interactions.AddRange(building.GetInteractions(coord));
        }
        
        return interactions;
    }

    public GridLocation(Vector2I coord)
    {
        this.coord = coord;
    }
}// EOF CLASs
