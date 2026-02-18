using Godot;
using System;

public partial class GoldNumber : RichTextLabel
{
    public override void _Ready()
    {
        MLobby.MLobbyPlayerEvents.OnGoldAmountChanged += (o,a)=>{ Text = $"[color=FFD700]{a.ToString("000")}[/color]"; };
    }
}
