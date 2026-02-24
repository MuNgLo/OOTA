using Godot;
using OOTA.Enums;
using OOTA.GameLogic;
using System;

namespace OOTA.HUD;

public partial class HUDBaseHealthBar : ProgressBar
{
    [Export] TEAM team;
    public override void _Ready()
    {
        GameStats.OnBaseDamage += (o, a) => { if (team == TEAM.LEFT) { Value = a[0]; } else { Value = a[1]; } };
    }
}// EOF CLASS
