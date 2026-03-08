using System;
using Godot;
using MLobby;
using OOTA.Enums;

namespace OOTA;

[GlobalClass]
public partial class OOTAPlayer : MLobbyPlayer
{
    public TEAM Team => (publicData as OOTAPublicData).Team;
    public int Gold => privateData.Gold;
    public float Health => (publicData as OOTAPublicData).Health;
    public float MaxHealth => (publicData as OOTAPublicData).MaxHealth;
    public bool IsReady => (publicData as OOTAPublicData).IsReady;
    public double NormalizedHealth => Math.Clamp(Health / MaxHealth, 0.0, 1.0);


    #region Unsynced
    public int TowerIDX { get => (publicData as OOTAPublicData).towerIDX; set => (publicData as OOTAPublicData).towerIDX = value; }
    #endregion 


    public bool CanTakeDamage
    {
        get => (publicData as OOTAPublicData).CanTakeDamage;
        set => (publicData as OOTAPublicData).CanTakeDamage = value;
    }
    /// <summary>
    /// Mode is can be changed only by the player who's mode it is
    /// </summary>
    public PLAYERMODE Mode
    {
        get => (publicData as OOTAPublicData).Mode;
        set => RequestSetMode(value);
    }
    public PLAYERSTATE State
    {
        get => (publicData as OOTAPublicData).State;
        set => SetState(value);
    }

    private void RequestSetMode(PLAYERMODE value)
    {
        RpcId(1, nameof(RPCHandleSetMode), (int)value);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCHandleSetMode(int modeAsInt)
    {
        long peer = Multiplayer.GetRemoteSenderId() == 0 ? 1 : Multiplayer.GetRemoteSenderId();
        if(PeerID != peer)
        {
            // Mode change from wrong player so it is ignored
            return;
        }

        if (Multiplayer.IsServer())
        {
            (publicData as OOTAPublicData).Mode = (PLAYERMODE)modeAsInt;
        }
    }

    PlayerAvatar avatar;
    //public PlayerAvatar Avatar { get => avatar; set => SetAvatar(value); }
    public PlayerAvatar Avatar { get => avatar; set => SetAvatar(value); }

    private void SetAvatar(PlayerAvatar value)
    {
        avatar = value;
        avatar.TreeExiting += ()=>{ avatar = null; };
        //Rpc(nameof(RPCSetAvatar), value is null ? null : value.GetPath());
    }
    /*[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCSetAvatar(NodePath nodePath)
    {
        avatar = GetNodeOrNull<PlayerAvatar>(nodePath);
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.AvatarChanged(avatar);
        }
    }*/

    public void AddHealth(int amount)
    {
        if (amount < 1) { return; }
        (publicData as OOTAPublicData).Health += amount;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0.0f) { return; }
        //GD.Print($"OOTAPlayer::TakeDamage({amount}) called and CanTakeDamage is {CanTakeDamage} health is [{(publicData as OOTAPublicData).Health}]");
        (publicData as OOTAPublicData).Health = (publicData as OOTAPublicData).Health - amount;
        if ((publicData as OOTAPublicData).Health <= 0)
        {
            //GD.Print($"Player::TakeDamage({amount}) Dying and avatar is null [{avatar is null}]");
            Die();
        }
    }
    public virtual void Die()
    {
        Core.Rules.PlayerDied(PeerID);
    }


    public void AddGold(int amount)
    {
        if (amount < 1) { return; }
        privateData.Gold += amount;
    }
    internal bool CanPay(int amount)
    {
        if (amount < 1) { return false; }
        return privateData.Gold >= amount;
    }

    internal bool Pay(int amount)
    {
        if (amount < 1) { return false; }
        if (CanPay(amount))
        {
            privateData.Gold -= amount;

            return true;
        }
        return false;
    }

    internal void SetGold(int startGold)
    {
        privateData.Gold = startGold;
    }

    internal void SetToReady()
    {
        (publicData as OOTAPublicData).IsReady = true;
    }
    internal void SetToNotReady()
    {
        (publicData as OOTAPublicData).IsReady = false;
    }

    internal void SetTeam(TEAM assignment)
    {
        (publicData as OOTAPublicData).Team = assignment;
    }

    internal void SetPeerID(int peerID)
    {
        (publicData as OOTAPublicData).PeerID = peerID;
    }

    internal void SetMaxHealth(float playerStartHealth)
    {
        (publicData as OOTAPublicData).MaxHealth = playerStartHealth;
    }

    internal void SetFullHealth()
    {
        (publicData as OOTAPublicData).Health = (publicData as OOTAPublicData).MaxHealth;
    }

    internal void SetHealth(float health)
    {
        (publicData as OOTAPublicData).Health = health;
    }

    private void SetState(PLAYERSTATE newState)
    {
        if (Multiplayer.IsServer())
        {
            if ((publicData as OOTAPublicData).State != newState)
            {
                (publicData as OOTAPublicData).State = newState;
            }
        }
    }
}// EOF CLASS
