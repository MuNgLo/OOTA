using Godot;
using System;
namespace MLobby;
/// <summary>
/// The object representing the player on the host. What is needed project to project differs but at the very least<br/>
/// the player should have the correct peerID set through the SetUpPlayer(). Which will happen when the PlayerManager spawns<br/>
/// in a new instance on the network.<br/>
/// <br/>
/// To adapt this to your project, either inherit or make your own class. Editing this class is not recommended as that will<br/>
/// get lost when updating the MLobby.
/// </summary>
[GlobalClass]
public partial class MLobbyPlayer : MLobbyBaseNode
{
    [ExportCategory("Player Data")]
    [Export] protected PrivateData privateData;
    [Export] protected PublicData publicData;

    public long PeerID => publicData.PeerID;
    public string PlayerName { get => publicData.PlayerName; internal set => publicData.PlayerName = value; }

    public void SetPeerID(long peerID)
    {
        publicData.PeerID = peerID;
    }
}// EOF CLASS
