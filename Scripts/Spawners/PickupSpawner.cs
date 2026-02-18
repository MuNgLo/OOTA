using System;
using Godot;
using MLogging;

[GlobalClass]
public partial class PickupSpawner : MultiplayerSpawner
{
    private static PickupSpawner ins;
    public enum PICKUPTYPE { GOLD, HEALTH }
    [Export] bool debug = false;
    [Export] PackedScene prefabGold;
    [Export] PackedScene prefabHealth;

    public override void _EnterTree()
    {
        ins = this;
        SpawnFunction = new Callable(this, nameof(SpawnPickup)); ;
    }

    public static UnitBaseClass SpawnThisPickup(SpawnPickupArgument args)
    {
        return ins.Spawn(args.AsSpawnArgs) as UnitBaseClass;
    }


    private Node SpawnPickup(Godot.Collections.Dictionary<string, Variant> args)
    {
        Pickup pickup = CreatePickup((PICKUPTYPE)args["type"].AsInt32());
        pickup.Team = (TEAM)args["team"].AsInt32();
        pickup.Position = args["pos"].AsVector3();
        pickup.Rotation = args["rot"].AsVector3();
        if (debug) { MLog.LogInfo($"PickupSpawner::SpawnPickup() Position[{args["pos"].AsVector3()}]"); }
        return pickup;
    }

    private Pickup CreatePickup(PICKUPTYPE type)
    {
        switch (type)
        {
            case PICKUPTYPE.GOLD:
                return prefabGold.Instantiate<Pickup>();
            case PICKUPTYPE.HEALTH:
            default:
                return prefabHealth.Instantiate<Pickup>();
        }
    }

   internal static void CleanUp()
    {
        foreach (Node child in ins.GetNode(ins.SpawnPath).GetChildren())
        {
            child.QueueFree();
        }
    }
    public struct SpawnPickupArgument
    {
        public TEAM team;
        public PICKUPTYPE type;
        public Vector3 rotation;
        public Vector3 position;

        public SpawnPickupArgument(TEAM team, PICKUPTYPE type, Vector3 rotation, Vector3 position)
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