using Godot;
using MLogging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PlayerSpace;

[GlobalClass]
public partial class PlayerManager : MultiplayerSpawner
{

    [ExportGroup("Player Prefab")]
    [Export] PackedScene playerPrefab;
    List<Player> players;

    /// <summary>
    /// All current players
    /// </summary>
    public List<Player> All => players;

    public event EventHandler OnPlayersChanged;

    public override void _EnterTree()
    {
        Core.players = this;
        players = new();
        SpawnFunction = new Callable(this, nameof(SpawnPlayerDataObject));
    }






    public override void _Ready()
    {
        ChildEnteredTree += WhenChildEnterTree;
        Core.Lobby.LobbyEvents.OnHostClosed += Cleanup;
        Core.Lobby.LobbyEvents.OnServerDisconnected += Cleanup;
        Core.Lobby.LobbyEvents.OnLeavingHost += Cleanup;
        Core.Lobby.LobbyEvents.OnLobbyMemberValidated += WhenLobbyMemberValidate;
    }

    private void WhenChildEnterTree(Node node)
    {
        if (!Multiplayer.IsServer())
        {
            // Process node for clientside player list
            if(node is Player pl)
            {
                if(!players.Exists(p=>p.PeerID == pl.PeerID))
                {
                    players.Add(pl);
                }
            }
        }
    }

    private void Cleanup(object sender, EventArgs e)
    {
        GD.Print($"PlayerManager::Cleanup()");
    }




    internal bool IsEveryoneReady()
    {

        for (int i = 0; i < players.Count; i++)
        {
            //GD.Print($"GameLogic::CheckIfEveryoneReady() -> [{players[i].peerID}]  [{(players[i].isReady ? "READY" : "NOTREADY")}]");
            if (players[i].IsReady == false)
            {
                return false;
            }
        }
        return true;
    }

    internal bool GetPlayer(int peerID, out Player player)
    {
        return GetPlayer((long)peerID, out player);
    }
    internal bool GetPlayer(long peerID, out Player player)
    {
        player = null;
        if (players.Exists(p => p.PeerID == peerID))
        {
            player = players.Find(p => p.PeerID == peerID);
        }
        return player is not null;
    }

    internal void SetStartResourcesOnAll(int startGold, int playerStartHealth)
    {
        foreach (Player player in Core.players.All)
        {
            player.SetGold(startGold);
            player.SetMaxHealth(playerStartHealth);
            player.SetFullHealth();
        }
    }

    #region Spawning
    internal void SpawnPlayers()
    {
        foreach (Player player in Core.players.All)
        {
            SpawnPlayer(player);
        }
    }
    internal void SpawnPlayer(Player player)
    {
        player.SetFullHealth();
        if(player.Avatar is not null)
        {
            MLog.LogError($"PlayerManager::SpawnPlayer({player.PeerID}) already has an avatar!", true);
            return;
        }
        player.Avatar = AvatarSpawner.SpawnThisAvatar(new AvatarSpawner.SpawnAvatarArgument(player)); // WARNING MIGHT BE RACE CONDITION BETWEEN GAME INSTANCES
    }
    internal async void SpawnPlayerWithDelay(int delay, long peerID)
    {
        await Task.Delay(delay);

        if (GetPlayer(peerID, out Player player))
        {
            SpawnPlayer(player);
        }
    }
    #endregion


    #region Data Object Spawning
    private void WhenLobbyMemberValidate(object sender, long peerID)
    {
        Godot.Collections.Dictionary<string, Variant> SpawnPlayerDataObjectArgs = new Godot.Collections.Dictionary<string, Variant>()
        {
            {"peerID", peerID }, 
            {"playerName", $"Player-{players.Count + 1}" } 
        };
        players.Add(Spawn(SpawnPlayerDataObjectArgs) as Player);
        Rpc(nameof(RPCRaisePlayersChanged));
    }
    private Player SpawnPlayerDataObject(Godot.Collections.Dictionary<string, Variant> args)
    {
        Player player = playerPrefab.Instantiate() as Player;
        player.SetPeerID(args["peerID"].AsInt32());
        player.PlayerName = args["playerName"].AsString();
        return player;
    }
    #endregion


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCRaisePlayersChanged()
    {
        OnPlayersChanged?.Invoke(null,null);
    }

    public void LocalRaisePlayersChanged()
    {
        OnPlayersChanged?.Invoke(null,null);
    }

    internal void SetNameOnPlayer(long peerID, string newName)
    {
        if(Core.players.GetPlayer(peerID, out Player player))
        {
            player.PlayerName = newName;
        }
    }
}// EOF CLASS
