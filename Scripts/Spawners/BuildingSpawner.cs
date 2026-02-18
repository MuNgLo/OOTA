using System;
using Godot;
using MLogging;

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
        BuildingBaseClass building = CreateBuilding((BUILDINGTYPE)args["type"].AsInt32());
        building.Team = (TEAM)args["team"].AsInt32();
        building.Position = args["pos"].AsVector3();
        building.Rotation = args["rot"].AsVector3();
        if (debug) { MLog.LogInfo($"BuildingSpawner::SpawnBuilding() Position[{args["pos"].AsVector3()}]"); }
        return building;
    }

    private BuildingBaseClass CreateBuilding(BUILDINGTYPE type)
    {
        switch (type)
        {
            case BUILDINGTYPE.BASE:
                return prefabBase.Instantiate<BuildingBaseClass>();
            case BUILDINGTYPE.TOWER:
            default:
                return prefabTower.Instantiate<BuildingBaseClass>();
        }
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
        public BUILDINGTYPE type;
        public Vector3 rotation;
        public Vector3 position;

        public SpawnBuildingArgument(TEAM team, BUILDINGTYPE type, Vector3 rotation, Vector3 position)
        {
            this.team = team;
            this.type = type;
            this.rotation = rotation;
            this.position = position;
        }

        public Godot.Collections.Dictionary<string, Variant> AsSpawnArgs => new Godot.Collections.Dictionary<string, Variant>()
        {
            {"team", (int)team },
            {"type", (int)type},
            {"rot", rotation},
            {"pos", position}
        };
    }// EOF STRUCT
}// EOF CLASS