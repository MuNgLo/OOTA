using Godot;
using OOTA.Units;
using System;

namespace OOTA;

public partial class UnitDeathTrigger : Area3D
{
    public override void _Ready()
    {
        BodyEntered += WhenBodyEntered;        
    }

    private void WhenBodyEntered(Node3D body)
    {
        if(Multiplayer.HasMultiplayerPeer() && !Multiplayer.IsServer()){return;}
        if(body is UnitBaseClass unit)
        {
            MLogging.MLog.LogInfo($"UnitDeathTrigger::  UNIT REMOVED!! Was out of bounce [{Name}]", true);
            Core.Rules.UnitDied(unit);
        }
    }
}// EOF CLASS
