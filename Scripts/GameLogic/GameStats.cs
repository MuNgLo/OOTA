using Godot;
using System;

public partial class GameStats : Node
{
    [Export] public int baseStartHealth = 5000;

     public int bhl = 0;
     public int bhr = 0;

    [Export] public int BaseHealthLeft { get => bhl; set { bhl = value; OnBaseDamage?.Invoke(null, [bhl, bhr]); } }
    [Export] public int BaseHealthRight { get => bhr; set { bhr = value; OnBaseDamage?.Invoke(null, [bhl, bhr]); } }

    /// <summary>
    /// Carries left/right base health
    /// </summary>
    public static event EventHandler<int[]> OnBaseDamage;

    public override void _EnterTree()
    {
        Core.Rules.OnGameStart += WhenGameStart;
    }

    private void WhenGameStart(object sender, EventArgs e)
    {
        BaseHealthLeft = baseStartHealth;
        BaseHealthRight = baseStartHealth;
        OnBaseDamage?.Invoke(null, [BaseHealthLeft, BaseHealthRight]);
    }

    internal void BaseDamage(TEAM team, int amount)
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
