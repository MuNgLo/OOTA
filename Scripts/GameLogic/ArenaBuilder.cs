using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Spawners;
using System;

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

    public override void _Ready()
    {
        GridNavigation.OnNavMeshRebuilt += WhenNavMeshRebuilt;
    }

    private void WhenNavMeshRebuilt(object sender, EventArgs e)
    {
        GD.Print("ArenaBuilder::WhenNavMeshRebuilt() called!");
    }

    public void BuildArena(int width, int depth)
    {
        SetArenaSize(width, depth);

        PositionWalls(width, depth);

        GridManager.InitGrid(width, depth);

        BuildFoundationsOnEnds();

        AddGoals();

        AddBases();

        GridNavigation.Rebuild();
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
        GD.Print($"ArenaBuilder::AddGoals() coord[{leftGoalCoord}] world[{GridManager.CoordToWorld(leftGoalCoord)}]");
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
            BuildFoundationOnLocation(location.Coord, TEAM.LEFT);
        }
        foreach (GridLocation location1 in GridManager.GetLastColumn())
        {
            BuildFoundationOnLocation(location1.Coord, TEAM.RIGHT);
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

    private void SetArenaSize(int width, int depth)
    {
        (arenaFloor.Mesh as QuadMesh).Size = new Vector2(width, depth);
        (arenaFloorCollider.Shape as BoxShape3D).Size = new Vector3(width, 6, depth);
    }
}// EOF CLASS
