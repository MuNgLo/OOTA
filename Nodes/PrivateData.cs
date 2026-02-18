using Godot;
using System;
namespace MLobby;
/// <summary>
/// This class should hold all the player info shared between player instance and host<br/>
/// Using a MultiPlayerSynchronizer Node the exported Properties down here replicates across network.<br/>
/// When they replicates and change the setter will raise events accordingly.
/// </summary>
[GlobalClass]
public partial class PrivateData : MLobbyBaseNode
{
    [Export] private MLobbyPlayer player;
    [Export] private MultiplayerSynchronizer synchronizer;
    [Export] public int Gold { get => gold; set => SetGold(value); }

    private int gold = 0;

    private long PeerID => player.PeerID;

    public override void _EnterTree()
    {
        // Limit the private data to the player it represents and the host
        synchronizer.PublicVisibility = false;
        synchronizer.SetVisibilityFor(1, true);
        synchronizer.SetVisibilityFor((int)PeerID, true);
        synchronizer.UpdateVisibility();
    }
    private void SetGold(int value)
    {
        gold = value;
        if(Multiplayer.GetUniqueId() == PeerID)
        {
            MLobbyPlayerEvents.RaiseOnGoldChanged(Gold);
        }
    }
}// EOF CLASS
