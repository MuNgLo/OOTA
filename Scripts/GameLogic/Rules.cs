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
    [Export] int reSpawnTimerMS = 5000;
    [ExportGroup("Player Start Resources")]
    [Export] int startGold = 10;
    [Export] int startHealth = 1000;

    [ExportGroup("Arena Size")]
    [Export] int ArenaWidth { get => arenaWidth; set => arenaWidth = Math.Clamp(value, 30, 100); }
    [Export] int ArenaDepth { get => arenaDepth; set => arenaDepth = Math.Clamp(value, 14, 60); }
    int arenaWidth = 40;
    int arenaDepth = 20;


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

    [ExportGroup("Unit sight blocking")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint unitSightBlockLayer;

    public float MaxLeft => arenaBuilder.MaxLeft;
    public float MaxRight => arenaBuilder.MaxRight;
    public float MaxTop => arenaBuilder.MaxTop;
    public float MaxBottom => arenaBuilder.MaxBottom;

    public event EventHandler OnGameStart;

    public override void _Ready()
    {
        LobbyEvents.OnHostClosed += Cleanup;
        LobbyEvents.OnHostSetupReady += WhenHostSetupReady;
        LobbyEvents.OnServerDisconnected += Cleanup;
        LobbyEvents.OnLeavingHost += Cleanup;
    }

    private void WhenHostSetupReady(object sender, ConnectedEventArguments e)
    {
        gameStats.GameState = GAMESTATE.SETUP;
    }

    private void Cleanup(object sender, EventArgs e)
    {
        GD.Print($"GameLogic::Cleanup()");
        ProjectileSpawner.CleanUp();
        BuildingSpawner.CleanUp();
        UnitSpawner.CleanUp();
        //AvatarSpawner.CleanUp();
        PickupSpawner.CleanUp();
    }

    public void UnitDied(UnitBaseClass unit)
    {
        if (!Multiplayer.IsServer()) { return; }
        RollForPickup(unit.GlobalPosition);
        Core.Players.GiveTeamGold(unit.Team == TEAM.LEFT ? TEAM.RIGHT : TEAM.LEFT, 1);
        unit.QueueFree();
    }

    public void BuildingDied(BuildingBaseClass building)
    {
        GD.Print($"Core::BuildingDied() path[{building.GetPath()}]");
        RollForPickup(building.GlobalPosition);
        Core.Grid.RemoveStructure(building);
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


    private async void StartGame()
    {
        //GD.Print($"GameLogic::StartGame()");
        AssignTeams();
        arenaBuilder.BuildArena(arenaWidth, arenaDepth);
        await Task.Delay(500);
        gameStats.GameState = GAMESTATE.PLAYING;
        Core.Players.SpawnPlayers();
        Core.Players.SetStartResourcesOnAll(startGold, startHealth);
        Core.Players.SetAllAsNotReady();
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
        Core.Players.KillEmAll();
        gameStats.GameState = GAMESTATE.POST;
        Cleanup(null, null);
        GD.Print($"Team WIN! [{team}]");
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
            if (tw.towerType == TOWERTYPE.FOUNDATION && !Core.Grid.TileIsFree(placerPoint)) { return; }
            if (player.Pay(tw.cost))
            {
                BuildingBaseClass building = BuildingSpawner.SpawnThisBuilding(new BuildingSpawner.SpawnBuildingArgument(
                    (TEAM)team,
                    towerIndex,
                    placerPoint
                    ));
                Core.Grid.PlaceStructure(building);
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
        //GD.Print($"RPCHandleReadyUp");
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out MLobbyPlayer player))
        {
            (player as OOTAPlayer).SetToReady();
        }
        if (Core.Players.IsEveryoneReady())
        {
            if (gameStats.GameState == GAMESTATE.SETUP)
            {
                StartGame();
            }
            else
            {
                Core.Players.SetAllAsNotReady();
                gameStats.GameState = GAMESTATE.SETUP;
            }
        }
    }

    public void PlayerDied(long peerID)
    {
        if (!Multiplayer.IsServer()) { return; }
        //RpcId(node.GetMultiplayerAuthority(), nameof(RPCHandOverAvatar), node.GetPath());
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player)) { player.State = PLAYERSTATE.DEAD; }
    }
    //[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void PlayerRequestHandOverAvatar(string nodePath)
    {
        RpcId(1, nameof(RPCAvatarHandedOver), nodePath);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCAvatarHandedOver(string nodePath)
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        /*
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            player.Avatar.QueueFree();
            player.Avatar = null;
        }*/
        PlayerAvatar ava = GetNode<PlayerAvatar>(nodePath);
        ava.SetMultiplayerAuthority(1);
        ava.QueueFree();
        if (gameStats.GameState == GAMESTATE.PLAYING)
        {
            Core.Players.SpawnPlayerWithDelay(reSpawnTimerMS, peerID);
        }
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
            Core.Players.GiveTeamGold(TEAM.LEFT, 1);
            gameStats.BaseDamage(TEAM.RIGHT, 1);
        }
        else if (unit.Team == TEAM.RIGHT)
        {
            Core.Players.GiveTeamGold(TEAM.RIGHT, 1);
            gameStats.BaseDamage(TEAM.LEFT, 1);
        }
        unit.QueueFree();
    }

    internal void RequestPlaceTower(int towerIDX, Vector2I coord)
    {
        RpcId(1, nameof(RPCRequestPlaceTower), towerIDX, coord);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    internal void RPCRequestPlaceTower(int towerIDX, Vector2I coord)
    {
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            TowerResource tw = Core.Rules.towers.GetTowerByIndex(towerIDX);
            GridLocation gridLocation = Core.Grid.GetGridLocation(coord);
            if (gridLocation.CanFit(tw))
            {
                if (player.CanPay(tw.cost))
                {
                    PlaceTower(peerID, player.Team, towerIDX, Core.Grid.CoordToWorld(coord));
                }
            }
        }
    }

    internal void Sell(Vector2I coord, int cost)
    {
        RpcId(1, nameof(RPCSell), coord, cost);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCSell(Vector2I coord, int cost)
    {
        GridLocation location = Core.Grid.GetGridLocation(coord);
        if (location is null) { return; }
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            if (location.Building is not null)
            {
                GD.Print("Sell Tower");
                player.AddGold(cost);
                location.Building = null;
                return;
            }
            if (location.Foundation is not null)
            {
                GD.Print("Sell Foundation");
                player.AddGold(cost);
                location.Foundation = null;
                return;
            }
        }
    }

    internal void Repair(Vector2I coord, int cost)
    {
        RpcId(1, nameof(RPCRepair), coord, cost);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCRepair(Vector2I coord, int cost)
    {
        GridLocation location = Core.Grid.GetGridLocation(coord);
        if (location is null) { return; }
        long peerID = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if (Core.Players.GetPlayer(peerID, out OOTAPlayer player))
        {
            if (location.Building is not null && location.Building.Health < location.Building.MaxHealth)
            {
                if (player.Pay(cost))
                {
                    GD.Print("Repair Tower");
                    location.Building.Health = location.Building.MaxHealth;
                    return;
                }
            }
            if (location.Foundation is not null)
            {
                if (player.Pay(cost))
                {
                    GD.Print("Repair Foundation");
                    location.Foundation.Health = location.Foundation.MaxHealth;
                    return;
                }
            }
        }
    }
}// EOF CLASS