using System;
using Godot;
using MLogging;
using OOTA.Enums;

namespace OOTA.Spawners;

[GlobalClass]
public partial class ProjectileSpawner : MultiplayerSpawner
{
    private static ProjectileSpawner ins;
    [Export] bool debug = false;

    public override void _EnterTree()
    {
        ins = this;
        SpawnFunction = new Callable(this, nameof(SpawnProjectile));
    }
    public static Projectile SpawnThisProjectile(SpawnProjectileArgument args)
    {
        return ins.Spawn(args.AsSpawnArgs) as Projectile;
    }
    private Node SpawnProjectile(Godot.Collections.Dictionary<string, Variant> args)
    {
        Projectile proj = GD.Load<PackedScene>(args["resourcePath"].AsString()).Instantiate() as Projectile;

        proj.Team = (TEAM)args["team"].AsInt32();
        proj.damage = args["damage"].AsInt32();
        proj.moveSpeed = args["speed"].AsDouble();
        proj.timeToLive = args["duration"].AsDouble();
        proj.Position = args["pos"].AsVector3();
        proj.Rotation = args["rot"].AsVector3();
        if (Multiplayer.IsServer())
        {
            proj.Ready += () => { proj.AddException(GetNode<CollisionObject3D>(args["exclude"].AsNodePath())); };
        }
        //if (debug) { MLog.LogInfo($"ProjectileSpawner::SpawnProjectile()  proj.damage[{proj.damage}]"); }
        return proj;
    }

    internal static void CleanUp()
    {
        foreach (Node child in ins.GetNode(ins.SpawnPath).GetChildren())
        {
            child.QueueFree();
        }
    }

    public struct SpawnProjectileArgument
    {
        public TEAM team;
        public int damage;
        public float speed;
        public float duration;
        public Vector3 rotation;
        public Vector3 position;
        public string resourcePath;
        public NodePath exclude;

        public SpawnProjectileArgument(string resourcePath, TEAM team, int damage, float speed, float duration, NodePath exclude, Vector3 rotation, Vector3 position)
        {
            this.resourcePath = resourcePath;
            this.team = team;
            this.damage = damage;
            this.speed = speed;
            this.duration = duration;
            this.exclude = exclude;
            this.rotation = rotation;
            this.position = position;
        }

        public Godot.Collections.Dictionary<string, Variant> AsSpawnArgs => new Godot.Collections.Dictionary<string, Variant>()
        {
            {"resourcePath", resourcePath },
            {"team", (int)team },
            {"damage", damage},
            {"speed", speed},
            {"duration", duration},
            {"exclude", exclude},
            {"rot", rotation},
            {"pos", position}
        };
    }// EOF STRUCT
}// EOF CLASS