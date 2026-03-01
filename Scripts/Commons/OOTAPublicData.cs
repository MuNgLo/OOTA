using Godot;
using System;
using MLobby;
using OOTA.Enums;

namespace OOTA;

/// <summary>
/// This class should hold all the player info shared with everyone on the network.<br/>
/// Using a MultiPlayerSynchronizer Node the exported Properties down here replicates across network.<br/>
/// When they replicates and change the setter will raise events accordingly.
/// </summary>
[GlobalClass]
public partial class OOTAPublicData : PublicData
{
    [Export] public bool IsReady { get => isReady; set => SetReady(value); }
    [Export] public float Health { get => health; set => SetHealth(value); }
    [Export] public float MaxHealth = 100;

    [Export] public bool CanTakeDamage { get => canTakeDamage; set => SetCanTakeDamage(value); }
    [Export] public PLAYERMODE Mode { get => mode; set => SetPlayerMode(value); }
    [Export] public PLAYERSTATE State { get => state; set => SetPlayerState(value); }

  

    PLAYERSTATE state = PLAYERSTATE.NONE;
    PLAYERMODE mode = PLAYERMODE.NONE;
    bool isReady = false;
    bool canTakeDamage = false;
    float health = 100;
    [Export] public TEAM Team { get => team; set => SetTeam(value); }
    TEAM team = TEAM.NONE;


    private void SetTeam(TEAM value)
    {
        team = value;
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.TeamChanged(team);
        }
    }
    private void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }
    private void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0, MaxHealth);
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.HealthChanged([Health, MaxHealth]);
        }
    }

    private void SetReady(bool value)
    {
        isReady = value;
        MLobbyPlayerEvents.RaiseOnPlayersChanged();
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.ReadyChanged(isReady);
        }
    }
    private void SetPlayerMode(PLAYERMODE newMode)
    {
        mode = newMode;
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.PlayerModeChanged(mode);
        }
    }
      private void SetPlayerState(PLAYERSTATE value)
    {
        state = value;
        MLobbyPlayerEvents.RaiseOnPlayersChanged();
        if (IsInsideTree() && Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.PlayerStateChanged(value);
        }
    }
}// EOF CLASS