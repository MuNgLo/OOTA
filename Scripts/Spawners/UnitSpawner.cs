using Godot;
using MLogging;
using OOTA.Enums;
using OOTA.Units;

namespace OOTA.Spawners;

[GlobalClass]
public partial class UnitSpawner : MultiplayerSpawner
{
    private static UnitSpawner ins;

    [Export] bool debug = false;

    public override void _EnterTree()
    {
        ins = this;
        SpawnFunction = new Callable(this, nameof(SpawnUnit)); ;
        if (debug) { Spawned += WhenNodeSpawned; }
    }

    public static UnitBaseClass SpawnThisUnit(SpawnUnitArguments args)
    {
        return ins.Spawn(args.AsSpawnArgs) as UnitBaseClass;
    }
    private void WhenNodeSpawned(Node node)
    {
        if (debug) { MLog.LogInfo($"UnitSpawner::WhenNodeSpawned() node path is [{node.GetPath()}]"); }
    }
    internal static void CleanUp()
    {
        foreach (Node child in ins.GetNode(ins.SpawnPath).GetChildren())
        {
            child.QueueFree();
        }
    }
    private Node SpawnUnit(Godot.Collections.Dictionary<string, Variant> args)
    {
        PackedScene prefab = GD.Load<PackedScene>(args["resourcePath"].AsString());
        UnitBaseClass unit = prefab.Instantiate() as UnitBaseClass;
        unit.Team = (TEAM)args["team"].AsInt32();
        unit.Position = args["pos"].AsVector3();
        unit.Rotation = args["rot"].AsVector3();
        if (debug) { MLog.LogInfo($"UnitSpawner::SpawnUnit() Position[{args["pos"].AsVector3()}]"); }
        return unit;
    }


    public struct SpawnUnitArguments
    {
        public TEAM team;
        public Vector3 rotation;
        public Vector3 position;
        public string resourcePath;

        public SpawnUnitArguments(TEAM team, Transform3D globalTransform, string resourcePath)
        {
            this.team = team;
            this.rotation = globalTransform.Basis.GetEuler();
            this.position = globalTransform.Origin;
            this.resourcePath = resourcePath;
        }

        public Godot.Collections.Dictionary<string, Variant> AsSpawnArgs => new Godot.Collections.Dictionary<string, Variant>()
        {
            {"team", (int)team},
            {"pos", position},
            {"rot", rotation},
            {"resourcePath", resourcePath}
        };
    }// EOF STRUCT
}// EOF CLASS
