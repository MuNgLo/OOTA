using Godot;
using OOTA.Resources;
using OOTA.Spawners;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OOTA.GameLogic;
using OOTA.Enums;

namespace OOTA.Buildings;

public partial class Base : BuildingBaseClass
{
    [ExportGroup("Spawns")]
    [Export] Node3D[] spawnPoints;

    public override void _Ready()
    {
        base._Ready();
        Waves.OnWaveSpawned += WhenWaveSpawned;
        TreeExiting += () => { Waves.OnWaveSpawned -= WhenWaveSpawned; };
    }

    private void WhenWaveSpawned(object sender, WaveDefinition e)
    {
        Spawn(e);
    }

    internal async void Spawn(WaveDefinition wave)
    {
        foreach (EnemySpawn spawns in wave.spawns)
        {
            for (int i = 0; i < spawns.amount; i++)
            {
                SpawnUnit(spawns.enemyPrefab.ResourcePath);
                await Task.Delay(150);
            }
        }
    }

    public override void TakeDamage(int amount)
    {
        if(!Multiplayer.IsServer()){return;}
        if (amount < 1) { return; }
        Core.Rules.gameStats.BaseDamage(team, amount);
    }

    private void SpawnUnit(string resourcePath )
    {
        Godot.Collections.Dictionary<string, Variant> args = new Godot.Collections.Dictionary<string, Variant>()
            {
                {"team", (int)team},
                {"pos", RNGSpawn() + Vector3.Forward * (team == TEAM.LEFT ? 1.0f : -1.0f)},
                {"rot", Vector3.Zero},
                {"resourcePath", resourcePath}
            };
        UnitSpawner.SpawnThisUnit(args);
    }

    private Vector3 RNGSpawn()
    {
        return spawnPoints[GD.RandRange(0, spawnPoints.Length - 1)].GlobalPosition;
    }
}// EOF CLASS
