using Godot;
using System;
namespace PlayerSpace;

[GlobalClass]
public partial class PublicData : Node
{
    [Export] public string PlayerName { get => playerName; set => SetPlayerName(value); }



    [Export] public TEAM Team { get => team; set => SetTeam(value); }

    [Export] public bool IsReady { get => isReady; set => SetReady(value); }

    [Export] public long PeerID { get; set; } = -1;

    [Export] public float Health { get => health; set => SetHealth(value); }
    [Export] public float MaxHealth = 100;

    string playerName = "NoName";
    bool isReady = false;
    TEAM team = TEAM.NONE;
    float health = 100;


    private void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0, MaxHealth);
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.HealthChanged([Health, MaxHealth]);
        }
    }

    private void SetPlayerName(string value)
    {
        if(playerName == value){ return;}
        playerName = value;
        //GD.Print("PlayerName set GOAL!");
        Core.players.LocalRaisePlayersChanged();
        //if (Multiplayer.GetUniqueId() == PeerID)
        //{
            //GD.Print("TODO add player name change event");
            //LocalLogic.HealthChanged([Health, MaxHealth]);
        //}
    }

    private void SetReady(bool value)
    {
        isReady = value;
        Core.players.LocalRaisePlayersChanged();
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.ReadyChanged(isReady);
        }
    }
    private void SetTeam(TEAM value)
    {
        team = value;
        if (Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.TeamChanged(team);
        }
    }
}// EOF CLASS
