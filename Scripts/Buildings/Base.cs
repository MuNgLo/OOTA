using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waves;

public partial class Base : BuildingBaseClass
{
    [ExportGroup("Spawns")]
    [Export] Node3D[] spawnPoints;

    internal async void Spawn(WaveDefinition wave, Material teamMaterial)
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
        UnitSpawner.SpawnThisUnit(args).TargetEnemyBase();
    }

    private Vector3 RNGSpawn()
    {
        return spawnPoints[GD.RandRange(0, spawnPoints.Length - 1)].GlobalPosition;
    }
}// EOF CLASS
