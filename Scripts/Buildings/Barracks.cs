using Godot;
using OOTA.Enums;
using OOTA.Spawners;
using System;

namespace OOTA.Buildings;

public partial class Barracks : Foundation
{
    [Export] PackedScene unitPrefab;
    [Export] ulong coolDownMS = 5000;
    [Export] Node3D spawnPoint;

    ulong tsLastSpawn = 0;

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
        if (!Multiplayer.IsServer()){return;}
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




}// EOF CLASS
