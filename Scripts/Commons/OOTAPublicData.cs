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

    bool isReady = false;
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
}// EOF CLASS