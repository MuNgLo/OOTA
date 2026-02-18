using Godot;
using MLobby;
using MLogging;
[GlobalClass]
public partial class AvatarSpawner : MultiplayerSpawner
{
    private static AvatarSpawner ins;
    [Export] bool debug = false;
    [Export] PackedScene packedLevel;

    [ExportGroup("Player Spawns")]
    [Export] Node3D spawnLeft;
    [Export] Node3D spawnRight;
    public override void _EnterTree()
    {
        ins = this;
        SpawnFunction = new Callable(this, nameof(SpawnAvatar)); ;
    }

    public static PlayerAvatar SpawnThisAvatar(SpawnAvatarArgument args)
    {
        return ins.Spawn(args.AsSpawnArgs) as PlayerAvatar;
    }
    internal static void CleanUp()
    {
        foreach (Node child in ins.GetNode(ins.SpawnPath).GetChildren())
        {
            child.QueueFree();
        }
    }
    private Node SpawnAvatar(Godot.Collections.Dictionary<string, Variant> args)
    {
        int peerID = args["peerID"].AsInt32();
        PlayerAvatar avatar = packedLevel.Instantiate() as PlayerAvatar;
        avatar.Name = $"P[{peerID}]";
        avatar.Team = (TEAM)args["team"].AsInt32();
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer pl)){
            avatar.player = pl;
        }
        avatar.Position = args["pos"].AsVector3();
        avatar.Rotation = args["rot"].AsVector3();
        avatar.SetMultiplayerAuthority(peerID, false);
        avatar.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(peerID, false);
        avatar.GetNode<Node3D>("WeaponPivot").SetMultiplayerAuthority(peerID, false);
        if (Multiplayer.GetUniqueId() != peerID)
        {
            avatar.Freeze = true;
        }
        if (debug) { MLog.LogInfo($"AvatarSpawner::SpawnAvatar() peerID[{peerID}] Position[{args["pos"].AsVector3()}] On peer [{Multiplayer.GetUniqueId()}]"); }
        return avatar;
    }



    public struct SpawnAvatarArgument
    {
        public long peerID;
        public TEAM team;

        public NodePath exclude;

        public SpawnAvatarArgument(OOTAPlayer player)
        {
            this.peerID = player.PeerID;
            this.team = player.Team;
        }

        public Godot.Collections.Dictionary<string, Variant> AsSpawnArgs => new Godot.Collections.Dictionary<string, Variant>()
            {
                {"peerID", peerID},
                {"team", (int)team},
                {"pos", team == TEAM.LEFT ? ins.spawnLeft.GlobalPosition : ins.spawnRight.GlobalPosition},
                {"rot", team == TEAM.LEFT ? ins.spawnLeft.GlobalRotation : ins.spawnRight.GlobalRotation}
            };
    }// EOF STRUCT
}// EOF CLASS
