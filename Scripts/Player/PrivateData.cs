using Godot;
using System;
namespace PlayerSpace;
[GlobalClass]
public partial class PrivateData : Node
{
    [Export] private Player player;
    [Export] public int Gold { get => gold; set => SetGold(value); }


    private int gold = 0;

    private long PeerID => player.PeerID;
    
    private void SetGold(int value)
    {
        gold = value;
        if(Multiplayer.GetUniqueId() == PeerID)
        {
            LocalLogic.GoldChanged(Gold);
        }
    }
}// EOF CLASS
