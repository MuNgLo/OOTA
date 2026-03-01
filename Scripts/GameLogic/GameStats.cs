using Godot;
using OOTA.Enums;
using System;

namespace OOTA.GameLogic;

public partial class GameStats : Node
{
    [Export] public GAMESTATE GameState { get => gamestate; set => SetGameState(value); }
    [Export] public double baseStartHealth = 100.0;

    GAMESTATE gamestate;
     public double bhl = 0;
     public double bhr = 0;


    public event EventHandler<GAMESTATE> OnGameStateChanged;

    private void SetGameState(GAMESTATE value)
    {
        if(gamestate != value)
        {
            gamestate = value;
            OnGameStateChanged?.Invoke(null, gamestate);
        }
    }

    [Export] public double BaseHealthLeft { get => bhl; set { bhl = value; OnBaseDamage?.Invoke(null, [BaseNormalizedHealthLeft, BaseNormalizedHealthRight]); } }
    [Export] public double BaseHealthRight { get => bhr; set { bhr = value; OnBaseDamage?.Invoke(null, [BaseNormalizedHealthLeft, BaseNormalizedHealthRight]); } }

    private double BaseNormalizedHealthLeft => Math.Clamp(BaseHealthLeft / baseStartHealth, 0.0, 1.0);
    private double BaseNormalizedHealthRight => Math.Clamp(BaseHealthRight / baseStartHealth, 0.0, 1.0);

    /// <summary>
    /// Carries left/right base health
    /// </summary>
    public static event EventHandler<double[]> OnBaseDamage;

    public override void _EnterTree()
    {
        Core.Rules.OnGameStart += WhenGameStart;
    }

    private void WhenGameStart(object sender, EventArgs e)
    {
        BaseHealthLeft = baseStartHealth;
        BaseHealthRight = baseStartHealth;
        OnBaseDamage?.Invoke(null, [BaseNormalizedHealthLeft, BaseNormalizedHealthRight]);
    }

    internal void BaseDamage(TEAM team, float amount)
    {
        if(team == TEAM.LEFT)
        {
            BaseHealthLeft -= amount;
        }
        if(team == TEAM.RIGHT)
        {
            BaseHealthRight -= amount;
        }
        CheckWinConditions();
    }

      private void CheckWinConditions()
    {
        if(BaseHealthLeft <= 0.0f){ Core.Rules.TeamWin(TEAM.RIGHT); }
        if(BaseHealthRight <= 0.0f){ Core.Rules.TeamWin(TEAM.LEFT); }
    }
}// EOF CLASS
