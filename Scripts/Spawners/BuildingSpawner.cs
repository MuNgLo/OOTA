using System;
using Godot;
using MLogging;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Resources;

namespace OOTA.Spawners;

[GlobalClass]
public partial class BuildingSpawner : MultiplayerSpawner
{
    private static BuildingSpawner ins;
    public enum BUILDINGTYPE { BASE, TOWER }
    [Export] bool debug = false;
    [Export] PackedScene prefabBase;
    [Export] PackedScene prefabTower;

    public override void _EnterTree()
    {
        ins = this;
        SpawnFunction = new Callable(this, nameof(SpawnBuilding)); ;
    }

    public static BuildingBaseClass SpawnThisBuilding(SpawnBuildingArgument args)
    {
        return ins.Spawn(args.AsSpawnArgs) as BuildingBaseClass;
    }

    private Node SpawnBuilding(Godot.Collections.Dictionary<string, Variant> args)
    {
        BuildingBaseClass building;
        if (args["towerIndex"].AsInt32() != -1)
        {
            TowerResource tw = Core.Rules.towers.GetTowerByIndex(args["towerIndex"].AsInt32());
            building = tw.towerPrefab.Instantiate<BuildingBaseClass>();
        }
        else
        {
            building = GD.Load<PackedScene>(args["resourcePath"].AsString()).Instantiate<BuildingBaseClass>();
        }


        building.Team = (TEAM)args["team"].AsInt32();
        building.Position = args["pos"].AsVector3();
        building.Rotation = building.towerType == TOWERTYPE.FOUNDATION ? Vector3.Zero : args["rot"].AsVector3();
        if (debug) { MLog.LogInfo($"BuildingSpawner::SpawnBuilding() Position[{args["pos"].AsVector3()}]"); }
        return building;
    }
    internal static void CleanUp()
    {
        foreach (Node child in ins.GetNode(ins.SpawnPath).GetChildren())
        {
            child.QueueFree();
        }
    }
    public struct SpawnBuildingArgument
    {
        public TEAM team;
        public int towerIndex;
        public Vector3 rotation;
        public Vector3 position;

        public string resourcePath;

        public SpawnBuildingArgument(TEAM team, int towerIndex, Vector3 position)
        {
            this.team = team;
            this.towerIndex = towerIndex;
            this.rotation = team == TEAM.LEFT ? new Vector3(0, Mathf.Pi * -0.5f, 0) : new Vector3(0, Mathf.Pi * 0.5f, 0);
            this.position = position;
        }

        public Godot.Collections.Dictionary<string, Variant> AsSpawnArgs => new Godot.Collections.Dictionary<string, Variant>()
        {
            {"team", (int)team },
            {"towerIndex", towerIndex},
            {"rot", rotation},
            {"pos", position},
            {"resourcePath", resourcePath}
        };
    }// EOF STRUCT
}// EOF CLASS