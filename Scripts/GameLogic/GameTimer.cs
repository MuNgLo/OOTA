using Godot;
using OOTA.Enums;
using System;

namespace OOTA.GameLogic;

public partial class GameTimer : Node
{
    private static GameTimer ins;
    [ExportGroup("Timer related")]
    [Export] bool runningTimer = false;
    [Export] float gameSpeed = 1.0f;
    [Export] float ticksPerSecond = 1.0f;
    [Export] float totalGameTime = 0.0f;
    int totalGameTicks = 0;
    float nextTickIn = 0.0f;

    public static float TotalGameTime => ins.totalGameTime;
    public static bool RunningTimer => ins.runningTimer;
    public static event EventHandler<int> OnGameTick;
    /// <summary>
    /// Carries gameSpeed,ticksPerSecond
    /// </summary>
    public static event EventHandler<float[]> OnGameTimingDecided;

    public override void _EnterTree()
    {
        ins = this;
    }
    public override void _Ready()
    {
        Core.Rules.OnGameStart += WhenGameStart;
        Core.Rules.gameStats.OnGameStateChanged += WhenGameStateChanged;

    }
    public override void _Process(double delta)
    {
        if (runningTimer)
        {
            RunTimer((float)delta);
        }
    }

    private void WhenGameStateChanged(object sender, GAMESTATE e)
    {
        if (e == GAMESTATE.PLAYING)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }



    private void WhenGameStart(object sender, EventArgs e)
    {
        OnGameTimingDecided?.Invoke(this, [gameSpeed, ticksPerSecond]);
    }


    private void StartTimer()
    {
        totalGameTime = 0.0f;
        totalGameTicks = 0;
        nextTickIn = 0.0f;
        runningTimer = true;
    }
    private void StopTimer()
    {
        runningTimer = false;
    }
    private void RunTimer(float delta)
    {
        totalGameTime += delta;
        nextTickIn -= delta;
        if (nextTickIn <= 0.0f)
        {
            RunTick(delta);
        }
    }
    private void RunTick(float delta)
    {
        nextTickIn = (1.0f / ticksPerSecond) + nextTickIn;
        totalGameTicks++;
        OnGameTick(null, totalGameTicks);
    }
}// EOF CLASS
