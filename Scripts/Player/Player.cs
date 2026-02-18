using Godot;
using System;
namespace PlayerSpace;

public partial class Player : Node
{
    [Export] PrivateData privateData;
    [Export] PublicData publicData;

    public long PeerID => publicData.PeerID;
    public TEAM Team => publicData.Team;
    public int Gold => privateData.Gold;
    public float Health => publicData.Health;
    public float MaxHealth => publicData.MaxHealth;
    public bool IsReady => publicData.IsReady;
    public double NormalizedHealth => Math.Clamp(Health / MaxHealth, 0.0, 1.0);



    PlayerAvatar avatar;
    public PlayerAvatar Avatar { get => avatar; set => SetAvatar(value); }
    public string PlayerName { get => publicData.PlayerName; internal set => publicData.PlayerName = value; }

    private void SetAvatar(PlayerAvatar value)
    {
        Rpc(nameof(RPCSetAvatar), value is null ? null : value.GetPath());
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCSetAvatar(NodePath nodePath)
    {
        avatar = GetNodeOrNull<PlayerAvatar>(nodePath);
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.AvatarChanged(avatar);
        }
    }

    public void SetUpPlayer(long peerID)
    {
        publicData.PeerID = peerID;
    }

    public void AddHealth(int amount)
    {
        if (amount < 1) { return; }
        publicData.Health += amount;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 1) { return; }
        publicData.Health = publicData.Health - amount;
        if (publicData.Health <= 0)
        {
            //GD.Print($"Player::TakeDamage({amount}) Dying and avatar is null [{avatar is null}]");
            Die();
        }
    }
    public virtual void Die()
    {
        Core.Rules.PlayerDied(avatar);
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
        publicData.IsReady = true;
    }

    internal void SetTeam(TEAM assignment)
    {
        GD.Print($"SetTeam");

        publicData.Team = assignment;
    }

    internal void SetPeerID(int peerID)
    {
        publicData.PeerID = peerID;
    }

    internal void SetMaxHealth(int playerStartHealth)
    {
        publicData.MaxHealth = playerStartHealth;
    }

    internal void SetFullHealth()
    {
        publicData.Health = publicData.MaxHealth;
    }
}// EOF CLASS
