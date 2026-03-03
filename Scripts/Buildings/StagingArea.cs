using Godot;
using OOTA.Interfaces;
using OOTA.Enums;
using OOTA.Units;
using OOTA.GameLogic;
using System;
using System.Collections.Generic;
using OOTA.Resources;

namespace OOTA.Buildings;

public partial class StagingArea : BuildingBaseClass, IMind
{
    [Export] Node3D center;

    public Node3D Center => center;

    List<UnitBaseClass> units;

    public override void _Ready()
    {
        units = new List<UnitBaseClass>();
        if (Multiplayer.IsServer())
        {
            base._Ready();
            Waves.OnWaveSpawned += WhenWaveSpawned;
        }
        Position += (Vector3.Right + Vector3.Back) * 0.5f;
    }
    public override void _ExitTree()
    {
        if (Multiplayer.IsServer())
        {
            Waves.OnWaveSpawned -= WhenWaveSpawned;
        }
    }

    private void WhenWaveSpawned(object sender, WaveDefinition e)
    {
        SendUnits();
    }

    public void SendUnits()
    {
        foreach (UnitBaseClass unit in units)
        {
            unit.ObjectiveState = OBJECTIVESTATE.ATTACK;
        }
    }

    public void BodyEnteredAggroRange(Node3D body)
    {
        if (body is UnitBaseClass unit && !units.Exists(p => p == unit))
        {
            units.Add(unit);
            unit.TreeExiting += () => { RemoveUnit(unit); };
        }
    }

    private void RemoveUnit(UnitBaseClass unit)
    {
        if (units.Exists(p => p == unit)) { units.RemoveAll(p => p == unit); }
    }

    public void BodyExitedAggroRange(Node3D body)
    {
        if (body is UnitBaseClass unit) { RemoveUnit(unit); }
    }
}// EOF CLASS
