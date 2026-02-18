using System;
using System.Threading.Tasks;
using Godot;
using MLobby;
using Waves;
[GlobalClass]
public partial class GameLogic : Node
{
    [ExportGroup("Player Start Resources")]
    [Export] int startGold = 10;
    [Export] int startHealth = 1000;

    [ExportGroup("Player Spawns")]
    [Export] Node3D baseLocationLeft;
    [Export] Node3D baseLocationRight;

    [ExportGroup("References")]
    [Export] public GameStats gameStats;
    [Export] PackedScene towerPrefab;


    [ExportGroup("Team Collision")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint leftTeamCollision;
    [Export(PropertyHint.Layers3DPhysics)]
    public uint rightTeamCollision;


    Base leftBase;
    Base rightBase;

    public Base LeftBase => leftBase;
    public Base RightBase => rightBase;
    public event EventHandler OnGameStart;

    public override void _Ready()
    {
        LobbyEvents.OnHostClosed += Cleanup;
        LobbyEvents.OnServerDisconnected += Cleanup;
        LobbyEvents.OnLeavingHost += Cleanup;
    }

    private void Cleanup(object sender, EventArgs e)
    {
        GD.Print($"GameLogic::Cleanup()");
        ProjectileSpawner.CleanUp();
        BuildingSpawner.CleanUp();
        UnitSpawner.CleanUp();
        AvatarSpawner.CleanUp();
        PickupSpawner.CleanUp();
    }

    public void SpawnWave(WaveDefinition wave)
    {
        leftBase.Spawn(wave, Core.TeamLeft);
        rightBase.Spawn(wave, Core.TeamRight);
    }

    private async void DelayedStart()
    {
        await Task.Delay(50);
        StartGame();
    }

    private async void StartGame()
    {
        //GD.Print($"GameLogic::StartGame()");
        AssignTeams();
        await Task.Delay(500);
        Core.Players.SpawnPlayers();
        SpawnBases();
        GameTimer.StartTimer();
        Core.Players.SetStartResourcesOnAll(startGold, startHealth);
        Rpc(nameof(RPCRaiseOnGameStart));
    }



    private void SpawnBases()
    {
        if (!Multiplayer.IsServer()) { return; }

        leftBase = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
            TEAM.LEFT,
            BuildingSpawner.BUILDINGTYPE.BASE,
            baseLocationLeft.GlobalRotation,
            baseLocationLeft.GlobalPosition
            )) as Base;
        rightBase = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
            TEAM.RIGHT,
            BuildingSpawner.BUILDINGTYPE.BASE,
            baseLocationRight.GlobalRotation,
            baseLocationRight.GlobalPosition
            )) as Base;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCRaiseOnGameStart()
    {
        OnGameStart?.Invoke(null, null);
    }
    private void AssignTeams()
    {

        TEAM assignment = GD.RandRange(0, 100) < 50 ? TEAM.LEFT : TEAM.RIGHT;
        foreach (MLobbyPlayer player in Core.Players.All)
        {
            (player as OOTAPlayer).SetTeam(assignment);
            assignment = assignment == TEAM.LEFT ? TEAM.RIGHT : TEAM.LEFT;
        }
    }
    public void TeamWin(TEAM team)
    {
        //GD.Print($"Team WIN! [{team}]");
    }


    public void PlaceTower(long peerID, TEAM team, BuildingSpawner.BUILDINGTYPE buildingType, Vector3 placerPoint)
    {
        RpcId(1, nameof(RPCPlaceTower), peerID, (int)team, (int)buildingType, placerPoint);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RPCPlaceTower(long peerID, int team, int buildingType, Vector3 placerPoint)
    {
        if (!Multiplayer.IsServer()) { return; }
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            if (player.Pay(10))
            {
                BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
                    (TEAM)team,
                    (BuildingSpawner.BUILDINGTYPE)buildingType,
                    Vector3.Zero,
                    placerPoint
                    ));
            }
        }
    }

    public void PlayerRequestReady()
    {
        RpcId(1, nameof(RPCHandleReadyUp));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCHandleReadyUp()
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            (player as OOTAPlayer).SetToReady();
        }
        if (Core.Players.IsEveryoneReady())
        {
            DelayedStart();
        }
    }

    public void PlayerDied(Node node)
    {
        if (!Multiplayer.IsServer()) { return; }
        RpcId(node.GetMultiplayerAuthority(), nameof(RPCHandOverAvatar), node.GetPath());

    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCHandOverAvatar(string nodePath)
    {
        Node ava = GetNode(nodePath);
        MultiplayerSynchronizer synchronizer = ava.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        synchronizer.PublicVisibility = false;
        synchronizer.UpdateVisibility();
        ava.SetMultiplayerAuthority(1);
        RpcId(1, nameof(RPCAvatarHandedOver), nodePath);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCAvatarHandedOver(string nodePath)
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            (player as OOTAPlayer).Avatar.QueueFree();
            (player as OOTAPlayer).Avatar = null;
        }
        Core.Players.SpawnPlayerWithDelay(1000, peerID);
    }

   

    internal void PlayerAddGold(int peerID, int gold)
    {
        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            (player as OOTAPlayer).AddGold(gold);
        }
    }

    internal void PlayerAddHealth(int peerID, int health)
    {
        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            (player as OOTAPlayer).AddHealth(health);
        }
    }

    internal bool CanPlayerPay(int peerID, int amount)
    {

        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            return (player as OOTAPlayer).CanPay(amount);
        }
        return false;
    }

    internal void PlayerRequestNameChange(string newName)
    {
        RpcId(1, nameof(RPCHandleNameChange), newName);
    }
    [Rpc( MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCHandleNameChange(string newName)
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        Core.Players.SetNameOnPlayer(peerID, newName);

    }
}// EOF CLASS