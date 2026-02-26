using Godot;
using Godot.Collections;
using OOTA.Resources;
using System;

namespace OOTA.GameLogic;

public partial class Waves : Node
{
    [Export] int finalWaveTiming = 10;

    [Export(PropertyHint.ResourceType, "WaveDefinition")]
    WaveDefinition[] waves;

    int lastWaveIndexSpawned = -1;

    public static event EventHandler<WaveDefinition> OnWaveSpawned;
    public static event EventHandler<WaveDefinition[]> OnWavesDecided;


    public override void _Ready()
    {
        base._Ready();
        GameTimer.OnGameTick += WhenGameTick;
        Core.Rules.OnGameStart += WhenGameStart;
    }

    private void WhenGameStart(object sender, EventArgs e)
    {
        OnWavesDecided?.Invoke(this, waves);
    }

    private void WhenGameTick(object sender, int tick)
    {
        int tickOffset = 0;
        if (lastWaveIndexSpawned > waves.Length - 1)
        {
            tickOffset = finalWaveTiming * (lastWaveIndexSpawned - waves.Length - 1);
        }
        WaveDefinition wave = GetWaveByIndex(lastWaveIndexSpawned + 1);
        if (wave.spawnsOnGameTick + tickOffset <= tick)
        {
            OnWaveSpawned?.Invoke(this, wave);
            lastWaveIndexSpawned++;
        }
    }

    public WaveDefinition GetWaveByIndex(int idx)
    {
        if (idx < 0) { GD.PushError("Index negative!"); return null; }

        idx = Mathf.Clamp(idx, 0, waves.Length - 1);

        return waves[idx];
    }
}// EOF CLASS
