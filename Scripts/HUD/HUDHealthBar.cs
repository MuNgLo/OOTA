using Godot;
using System;

namespace OOTA.HUD;

public partial class HUDHealthBar : ProgressBar
{
       public override void _Ready()
    {
        LocalLogic.OnHealthChanged += (o,a)=>{ MaxValue = a[1]; Value = a[0]; };
    }
}// EOF CLASS
