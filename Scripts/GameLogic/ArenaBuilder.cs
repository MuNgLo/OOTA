using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Spawners;
using System;
using System.Collections.Generic;

namespace OOTA.GameLogic;

public partial class ArenaBuilder : Node
{
    [Export] MeshInstance3D arenaFloor;
    [Export] CollisionShape3D arenaFloorCollider;

    [ExportGroup("References")]
    [Export] StaticBody3D wallNorth;
    [Export] StaticBody3D wallSouth;
    [Export] StaticBody3D wallEast;
    [Export] StaticBody3D wallWest;

    public float MaxLeft => wallWest.GlobalPosition.X;
    public float MaxRight => wallEast.GlobalPosition.X;
    public float MaxTop => wallNorth.GlobalPosition.Z;
    public float MaxBottom => wallSouth.GlobalPosition.Z;

    public override void _Ready()
    {
        //GridNavigation.OnNavMeshRebuilt += WhenNavMeshRebuilt;
    }

    private void WhenNavMeshRebuilt(object sender, EventArgs e)
    {
        GD.Print("ArenaBuilder::WhenNavMeshRebuilt() called!");
    }

    public void BuildArena(int width, int depth)
    {
        Rpc(nameof(RPCBuildArena), width, depth);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RPCBuildArena(int width, int depth)
    {
        SetArenaSize(width, depth);

        PositionWalls(width, depth);

        GridManager.InitGrid(width, depth);

        if (Multiplayer.IsServer())
        {
            BuildArenaMeleeBarracks();
            BuildStagingAreas();
            BuildFoundationsOnEnds();
            AddGoals();
            //AddBases();
            GridNavigation.Rebuild();
        }
    }
    private void BuildArenaMeleeBarracks()
    {
        List<GridLocation> locations = GridManager.GetFirstColumn();
        int idx = locations.Count / 3;
        BuildBarracksOnLocations(locations[idx].Coord, locations[idx].Coord + Vector2I.Down, locations[idx].Coord + Vector2I.Down * 2, TEAM.LEFT);
        locations = GridManager.GetLastColumn();
        idx = locations.Count / 3;
        BuildBarracksOnLocations(locations[idx].Coord, locations[idx].Coord + Vector2I.Down, locations[idx].Coord + Vector2I.Down * 2, TEAM.RIGHT);
    }

    private void BuildStagingAreas()
    {
        List<GridLocation> locations = GridManager.GetColumn(5);
        int idx = locations.Count / 2;
        BuildStagingAreaOnLocation(locations[idx].Coord, TEAM.LEFT);

        locations = GridManager.GetColumn(-7);
        idx = locations.Count / 2;
        BuildStagingAreaOnLocation(locations[idx].Coord, TEAM.RIGHT);
    }

    private void BuildStagingAreaOnLocation(Vector2I coordMelee, TEAM team)
    {
        BuildingBaseClass buildingMelee = BuildingSpawner.SpawnThisBuilding(
            new BuildingSpawner.SpawnBuildingArgument(team, -1, GridManager.CoordToWorld(coordMelee))
            { resourcePath = "res://Scenes/Buildings/StagingArea.tscn", rotation = Vector3.Zero });
        GridManager.PlaceStructure(buildingMelee);
        GridManager.GetGridLocation(coordMelee + Vector2I.Right).canBuild = false;
        GridManager.GetGridLocation(coordMelee + Vector2I.Down).canBuild = false;
        GridManager.GetGridLocation(coordMelee + Vector2I.Down + Vector2I.Right).canBuild = false;
    }

    private void AddBases()
    {
        Vector2I leftBaseCoord = GridManager.LeftGoalCoord() + new Vector2I(2, 0);
        Vector2I rightBaseCoord = GridManager.RightGoalCoord() + new Vector2I(-2, 0);

        BuildingSpawner.SpawnThisBuilding(
            new BuildingSpawner.SpawnBuildingArgument(
            TEAM.LEFT,
            -1,
            GridManager.CoordToWorld(leftBaseCoord)
            )
            { resourcePath = "res://Scenes/Buildings/Base.tscn" });

        BuildingSpawner.SpawnThisBuilding(
            new BuildingSpawner.SpawnBuildingArgument(
            TEAM.RIGHT,
            -1,
            GridManager.CoordToWorld(rightBaseCoord)
            )
            { resourcePath = "res://Scenes/Buildings/Base.tscn" });

    }


    private void AddGoals()
    {
        Vector2I leftGoalCoord = GridManager.LeftGoalCoord();
        Vector2I rightGoalCoord = GridManager.RightGoalCoord();
        BuildingBaseClass leftGoal = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
       TEAM.LEFT,
       -1,
       GridManager.CoordToWorld(leftGoalCoord)
       )
        { resourcePath = "res://Scenes/Buildings/Goal.tscn" });

        BuildingBaseClass rightGoal = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
       TEAM.RIGHT,
       -1,
       GridManager.CoordToWorld(rightGoalCoord)
       )
        { resourcePath = "res://Scenes/Buildings/Goal.tscn" });

        GridManager.PlaceStructure(leftGoal);
        GridManager.PlaceStructure(rightGoal);
        //GD.Print($"ArenaBuilder::AddGoals() coord[{leftGoalCoord}] world[{GridManager.CoordToWorld(leftGoalCoord)}]");
    }


    private void PositionWalls(int width, int depth)
    {
        wallWest.GlobalPosition = Vector3.Left * width * 0.5f;
        wallEast.GlobalPosition = Vector3.Right * width * 0.5f;
        wallNorth.GlobalPosition = Vector3.Forward * depth * 0.5f;
        wallSouth.GlobalPosition = Vector3.Back * depth * 0.5f;
    }

    private void BuildFoundationsOnEnds()
    {
        foreach (GridLocation location in GridManager.GetFirstColumn())
        {
            if (location.IsFree)
            {
                BuildFoundationOnLocation(location.Coord, TEAM.LEFT);
            }
        }
        foreach (GridLocation location1 in GridManager.GetLastColumn())
        {
            if (location1.IsFree)
            {
                BuildFoundationOnLocation(location1.Coord, TEAM.RIGHT);
            }
        }
    }

    private void BuildFoundationOnLocation(Vector2I coord, TEAM team)
    {
        BuildingBaseClass building = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
               team,
               0,
               GridManager.CoordToWorld(coord)
               ));
        building.canTakeDamage = false;
        GridManager.PlaceStructure(building);
        //GD.Print($"ArenaBuilder::BuildFoundationOnLocation() coord[{coord}] world[{GridManager.CoordToWorld(coord)}]");
    }

    private void BuildBarracksOnLocations(Vector2I coordMelee, Vector2I coordSupport, Vector2I coordRanged, TEAM team)
    {
        Vector2I blockOffset = team == TEAM.LEFT ? Vector2I.Right : Vector2I.Left;
        BuildingBaseClass buildingMelee = BuildingSpawner.SpawnThisBuilding(
            new BuildingSpawner.SpawnBuildingArgument(team, -1, GridManager.CoordToWorld(coordMelee))
            { resourcePath = "res://Scenes/Buildings/MeleeBarracks.tscn" });
        GridManager.PlaceStructure(buildingMelee);
        GridManager.GetGridLocation(coordMelee + blockOffset).canBuild = false;


        BuildingBaseClass buildingSupport = BuildingSpawner.SpawnThisBuilding(
          new BuildingSpawner.SpawnBuildingArgument(team, -1, GridManager.CoordToWorld(coordSupport))
          { resourcePath = "res://Scenes/Buildings/SupportBarracks.tscn" });
        GridManager.PlaceStructure(buildingSupport);
        GridManager.GetGridLocation(coordSupport + blockOffset).canBuild = false;


        BuildingBaseClass buildingRanged = BuildingSpawner.SpawnThisBuilding(
          new BuildingSpawner.SpawnBuildingArgument(team, -1, GridManager.CoordToWorld(coordRanged))
          { resourcePath = "res://Scenes/Buildings/RangedBarracks.tscn" });
        GridManager.PlaceStructure(buildingRanged);
        GridManager.GetGridLocation(coordRanged + blockOffset).canBuild = false;

    }

    private void SetArenaSize(int width, int depth)
    {
        (arenaFloor.Mesh as QuadMesh).Size = new Vector2(width, depth);
        (arenaFloorCollider.Shape as BoxShape3D).Size = new Vector3(width, 6, depth);
    }
}// EOF CLASS
