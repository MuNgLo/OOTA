using Godot;
using System;

namespace OOTA.Buildings;

public partial class RangedTowerAnimations : AnimationPlayer
{
    [Export] RangedTower tower;

    public override void _Ready()
    {
        tower.OnAttack += WhenAttack;
        tower.OnReload += WhenReload;
    }

    void WhenAttack(object sender, float speed)
    {
        Play("launch", customSpeed: speed);
    }
    void WhenReload(object sender, float speed)
    {
        Play("PostLaunch", customSpeed: speed);
    }
}// EOF CLASS
