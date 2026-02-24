using Godot;
using System;

namespace OOTA.HUD;

public partial class HUDGoldNumber : RichTextLabel
{
    public override void _Ready()
    {
        MLobby.MLobbyPlayerEvents.OnGoldAmountChanged += (o,a)=>{ Text = $"[color=FFD700]{a.ToString("000")}[/color]"; };
    }
}
