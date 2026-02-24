using System;
using System.Threading.Tasks;
using Godot;
using MLobby;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Resources;
using OOTA.Spawners;
using OOTA.Units;

namespace OOTA.GameLogic;

[GlobalClass]
public partial class Rules : Node
{
    [ExportGroup("Player Start Resources")]
    [Export] int startGold = 10;
    [Export] int startHealth = 1000;

    [ExportGroup("Arena Size")]
    [Export] int ArenaWidth { get => arenaWidth; set => arenaWidth = Math.Clamp(value, 30, 100); }
    [Export] int ArenaDepth { get => arenaDepth; set => arenaDepth = Math.Clamp(value, 14, 60); }
    int arenaWidth = 40;
    int arenaDepth = 20;


    [ExportGroup("Player Spawns")]
    [Export] Node3D baseLocationLeft;
    [Export] Node3D baseLocationRight;

    [ExportGroup("References")]
    [Export] public GameStats gameStats;
    [Export] public Towers towers;
    [Export] PackedScene towerPrefab;
    [Export] ArenaBuilder arenaBuilder;
    [Export] PackedScene goldPrefab;
    [Export] PackedScene healthPrefab;


    [ExportGroup("Team Collision")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint leftTeamCollision;
    [Export(PropertyHint.Layers3DPhysics)]
    public uint rightTeamCollision;

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

    public void UnitDied(UnitBaseClass unit)
    {
        if (!Multiplayer.IsServer()) { return; }
        RollForPickup(unit.GlobalPosition);
        unit.QueueFree();
    }

    public void BuildingDied(BuildingBaseClass building)
    {
        GD.Print($"Core::BuildingDied() path[{building.GetPath()}]");
        RollForPickup(building.GlobalPosition);
        GridManager.RemoveStructure(building);
        building.QueueFree();
    }
    private void RollForPickup(Vector3 worldPosition)
    {
        int roll = GD.RandRange(0, 100);
        if (roll < 40)
        {
            PickupSpawner.SpawnThisPickup(new PickupSpawner.SpawnPickupArgument(TEAM.NONE, PickupSpawner.PICKUPTYPE.GOLD, worldPosition));
        }
        else if (roll < 60)
        {
            PickupSpawner.SpawnThisPickup(new PickupSpawner.SpawnPickupArgument(TEAM.NONE, PickupSpawner.PICKUPTYPE.HEALTH, worldPosition));
        }
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
        arenaBuilder.BuildArena(arenaWidth, arenaDepth);

        GameTimer.StartTimer();
        Core.Players.SetStartResourcesOnAll(startGold, startHealth);
        Rpc(nameof(RPCRaiseOnGameStart));
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


    public void PlaceTower(long peerID, TEAM team, int towerIndex, Vector3 placerPoint)
    {
        RpcId(1, nameof(RPCPlaceTower), peerID, (int)team, towerIndex, placerPoint);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RPCPlaceTower(long peerID, int team, int towerIndex, Vector3 placerPoint)
    {
        if (!Multiplayer.IsServer()) { return; }



        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            TowerResource tw = Core.Rules.towers.GetTowerByIndex(towerIndex);
            if (tw.towerType == TOWERTYPE.FOUNDATION && !GridManager.TileIsFree(placerPoint)) { return; }
            if (player.Pay(tw.cost))
            {
                BuildingBaseClass building = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
                    (TEAM)team,
                    towerIndex,
                    placerPoint
                    ));
                GridManager.PlaceStructure(building);
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
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCHandleNameChange(string newName)
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        Core.Players.SetNameOnPlayer(peerID, newName);

    }

    internal void UnitReachedGoal(UnitBaseClass unit)
    {
        if (unit.Team == TEAM.LEFT)
        {
            gameStats.BaseDamage(TEAM.RIGHT, 1);
        }
        else if (unit.Team == TEAM.RIGHT)
        {
            gameStats.BaseDamage(TEAM.LEFT, 1);
        }
        unit.QueueFree();
    }
}// EOF CLASS