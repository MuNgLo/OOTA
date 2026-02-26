using Godot;
using MLobby;
using MLogging;
using OOTA.Enums;
using OOTA.Spawners;
using System;
using System.Threading.Tasks;

namespace OOTA;

partial class OOTAPlayerManager : PlayerManager
{
    private OOTAPlayer localPlayer;
    public OOTAPlayer LocalPlayer { get => localPlayer; }

    private protected override void WhenChildEnterTree(Node node)
    {
        base.WhenChildEnterTree(node);
        if (node is OOTAPlayer ootaPlayer && ootaPlayer.PeerID == Multiplayer.GetUniqueId())
        {
            localPlayer = ootaPlayer;
        }
    }


    public bool GetPlayer(long peerID, out OOTAPlayer player)
    {
        return GetPlayer((int)peerID, out player);
    }
    public bool GetPlayer(int peerID, out OOTAPlayer player)
    {
        if (GetPlayer((long)peerID, out MLobbyPlayer pl))
        {
            player = pl as OOTAPlayer;
            return true;
        }
        player = null;
        return false;
    }


    internal bool IsEveryoneReady()
    {

        for (int i = 0; i < players.Count; i++)
        {
            //GD.Print($"GameLogic::CheckIfEveryoneReady() -> [{players[i].peerID}]  [{(players[i].isReady ? "READY" : "NOTREADY")}]");
            if ((players[i] as OOTAPlayer).IsReady == false)
            {
                return false;
            }
        }
        return true;
    }

    internal void SetStartResourcesOnAll(int startGold, int playerStartHealth)
    {
        foreach (MLobbyPlayer player in players)
        {
            (player as OOTAPlayer).SetGold(startGold);
            (player as OOTAPlayer).SetMaxHealth(playerStartHealth);
            (player as OOTAPlayer).SetFullHealth();
        }
    }

    #region Spawning
    internal void SpawnPlayers()
    {
        foreach (MLobbyPlayer player in players)
        {
            SpawnPlayer(player as OOTAPlayer);
        }
    }
    internal void SpawnPlayer(OOTAPlayer player)
    {
        player.SetFullHealth();
        if (player.Avatar is not null)
        {
            MLog.LogError($"PlayerManager::SpawnPlayer({player.PeerID}) already has an avatar!", true);
            return;
        }
        player.Avatar = AvatarSpawner.SpawnThisAvatar(new AvatarSpawner.SpawnAvatarArgument(player)); // WARNING MIGHT BE RACE CONDITION BETWEEN GAME INSTANCES
    }
    internal async void SpawnPlayerWithDelay(int delay, long peerID)
    {
        await Task.Delay(delay);

        if (GetPlayer(peerID, out MLobbyPlayer player))
        {
            SpawnPlayer(player as OOTAPlayer);
        }
    }

    internal void GiveTeamGold(TEAM team, int amount)
    {
        foreach (MLobbyPlayer player in players)
        {
            if ((player as OOTAPlayer).Team == team)
            {
                (player as OOTAPlayer).AddGold(amount);
            }
        }
    }
    #endregion
}// EOF CLASS