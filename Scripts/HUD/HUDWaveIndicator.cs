using Godot;
using OOTA.GameLogic;
using OOTA.Resources;
using System;

namespace OOTA.HUD;

public partial class HUDWaveIndicator : Control
{
    [Export] int dashWidth = 6;

    [ExportGroup("References")]
    [Export] HFlowContainer flowContainer;
    [Export] PackedScene prefabDash;
    [Export] PackedScene prefabWave;

    float moveSpeed = 0.0f;

    public override void _Ready()
    {
        Waves.OnWavesDecided += WhenWavesDecided;
        GameTimer.OnGameTimingDecided += WhenGameTimingDecided;
    }
    public override void _Process(double delta)
    {
        if (GameTimer.RunningTimer)
        {
            flowContainer.Position += Vector2.Left * moveSpeed * (float)delta;
        }
    }
    private void WhenGameTimingDecided(object sender, float[] e)
    {
       moveSpeed = dashWidth * e[1] * e[0];
    }

    private void WhenWavesDecided(object sender, WaveDefinition[] waves)
    {
        foreach (WaveDefinition wave in waves)
        {
            BuildWave(wave);
        }
    }

    private void BuildWave(WaveDefinition e)
    {
        for (int i = 0; i < e.spawnsOnGameTick; i++)
        {
            if(i >= flowContainer.GetChildCount())
            {
                AddDash();
            }
        }
        AddWave(e);
        flowContainer.Position = Vector2.Left * dashWidth;
    }

    private void AddDash()
    {
        Label lbl = prefabDash.Instantiate<Label>();
        lbl.CustomMinimumSize = new Vector2(dashWidth, lbl.Size.Y);
        lbl.TooltipText = $"Dash at tick {flowContainer.GetChildCount()}";
        flowContainer.AddChild(lbl);
    }
    private void AddWave(WaveDefinition wave)
    {
        HUDWaveIcon waveIcon = prefabWave.Instantiate<HUDWaveIcon>();
        waveIcon.SetDashToTrack(flowContainer.GetChild(flowContainer.GetChildCount() - 1) as Label);
        AddChild(waveIcon);
    }
}// EOF CLASS
