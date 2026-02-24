using Godot;
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
    public static event EventHandler<int> OnGameTick;

    public override void _EnterTree()
    {
        ins = this;
    }

    public override void _Process(double delta)
    {
        if (runningTimer)
        {
            RunningTimer((float)delta);
        }
    }
    public static void StartTimer()
    {
        ins.totalGameTime = 0.0f;
        ins.totalGameTicks = 0;
        ins.nextTickIn = 0.0f;
        ins.runningTimer = true;
    }
    private void RunningTimer(float delta)
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
