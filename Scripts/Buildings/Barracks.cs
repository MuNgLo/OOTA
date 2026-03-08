using Godot;
using OOTA.Enums;
using OOTA.Spawners;
using System;
using System.Collections.Generic;

namespace OOTA.Buildings;

public partial class Barracks : Foundation
{
    [Export] PackedScene unitPrefab;
    [Export] ulong coolDownMS = 5000;
    [Export] Node3D spawnPoint;



    ulong tsLastSpawn = 0;

    public int ProductionUpgradeCost => upgradeBaseCost * currentTier;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            base._Ready();
            Core.Rules.OnGameStart += WhenGameStarts;
            canTakeDamage = false;
        }
    }
    public override void _Process(double delta)
    {
        if (!Multiplayer.IsServer()) { return; }
        if (Core.Rules.gameStats.GameState == GAMESTATE.PLAYING && Time.GetTicksMsec() > tsSpawnMS + coolDownMS) { SpawnUnit(); }
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
        tsSpawnMS = Time.GetTicksMsec();
    }

    private void SpawnUnit()
    {
        tsSpawnMS = Time.GetTicksMsec();
        UnitSpawner.SpawnThisUnit(new UnitSpawner.SpawnUnitArguments(team, spawnPoint.GlobalTransform, unitPrefab.ResourcePath));
    }


    #region Upgrade stuff
    public override List<PlayerActionStruct> GetInteractions(Vector2I coord)
    {
        List<PlayerActionStruct> interActions = new List<PlayerActionStruct>();
        if (currentTier < maxTier)
        {
            interActions.Add(UpgradeProductionAction(coord));
        }
        return interActions;
    }

    private PlayerActionStruct UpgradeProductionAction(Vector2I coord)
    {
        return new PlayerActionStruct()
        {
            Coord = coord,
            ToolTip = "Production",
            Cost = ProductionUpgradeCost,
            modulate = Colors.RoyalBlue,
            texture = ResourceLoader.Load<Texture2D>("res://Images/Icons/BuildTime.png"),
            action = () => { Core.Rules.PlayerRequestUpgradeProduction(coord); }
        };
    }

    public void UpgradeProduction()
    {
        if (Multiplayer.IsServer())
        {
            coolDownMS -= 200;
            currentTier++;
        }
    }

    #endregion
}// EOF CLASS
