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
        if(!Multiplayer.IsServer()){return;}
        base._Ready();
        Waves.OnWaveSpawned += WhenWaveSpawned;
        TreeExiting += () => { Waves.OnWaveSpawned -= WhenWaveSpawned; };
    }

    private void WhenWaveSpawned(object sender, WaveDefinition e)
    {
        //GD.Print($"Base spawning for team[{Team}]");
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
    [Obsolete("Verify this overrides the interface default method")]
    public void TakeDamage(float amount)
    {
        if(!Multiplayer.IsServer()){return;}
        if (amount < 1) { return; }
        Core.Rules.gameStats.BaseDamage(team, amount);
    }

    private void SpawnUnit(string resourcePath )
    {
        UnitSpawner.SpawnThisUnit(new UnitSpawner.SpawnUnitArguments(team, RNGSpawn().GlobalTransform, resourcePath));
    }

    private Node3D RNGSpawn()
    {
        return spawnPoints[GD.RandRange(0, spawnPoints.Length - 1)];
    }
}// EOF CLASS
