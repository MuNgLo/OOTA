using Godot;
using System;
namespace MLobby;
/// <summary>
/// This class should hold all the player info shared with everyone on the network.<br/>
/// Using a MultiPlayerSynchronizer Node the exported Properties down here replicates across network.<br/>
/// When they replicates and change the setter will raise events accordingly.
/// </summary>
[GlobalClass]
public partial class PublicData : MLobbyBaseNode
{
    [Export] public long PeerID { get; set; } = -1;
    [Export] public string PlayerName { get => playerName; set => SetPlayerName(value); }
    string playerName = "NoName";

    private void SetPlayerName(string value)
    {
        if (playerName == value) { return; }
        playerName = value;
        if(!IsInsideTree()){ return;}
        //GD.Print("PlayerName set GOAL!");
        MLobbyPlayerEvents.RaiseOnPlayersChanged();
        if ( Multiplayer.GetUniqueId() == PeerID)
        {
            GD.Print("TODO add player name change event");
            //LocalLogic.HealthChanged([Health, MaxHealth]);
        }
    }
}// EOF CLASS
