using Godot;
using System;
namespace HUD;
public partial class HUDBaseHealthBar : ProgressBar
{
    [Export] TEAM team;
    public override void _Ready()
    {
        GameStats.OnBaseDamage += (o, a) => { if (team == TEAM.LEFT) { Value = a[0]; } else { Value = a[1]; } };
    }
}// EOF CLASS
