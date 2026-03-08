using Godot;
using System;

namespace OOTA.HUD;

public partial class HudActionEntry : Control
{
    [Export] TextureRect icon;
    [Export] RichTextLabel cost;

    HUDInteractMenu interactMenu;
    PlayerActionStruct playerAction;
    int actionIndex = -1;

    public void SetupEntry(HUDInteractMenu iMenu, PlayerActionStruct pAction, int idx)
    {
        interactMenu = iMenu;
        playerAction = pAction;
        actionIndex = idx;
        GD.Print($"HudActionEntry::SetupEntry({playerAction.ToolTip}) icon[{playerAction.texture.ResourceName}]");
        icon.Texture = playerAction.texture;
        icon.Modulate = playerAction.modulate;
        icon.TooltipText = playerAction.ToolTip;

        if(playerAction.ToolTip == "Sell")
        {
            cost.Text = "+" + pAction.Cost.ToString();
        }
        else
        {
            cost.Text = pAction.Cost.ToString();
        }


        icon.MouseEntered += ()=>{ iMenu.selectedButton = actionIndex; };
        icon.MouseExited +=  ()=>{ iMenu.selectedButton = -1; };
    }


}// EOF CLASS
