using Godot;
using System;

namespace OOTA.HUD;

public partial class HudActionEntry : Control
{
    [Export] TextureRect icon;
    [Export] TextureRect frame;
    [Export] RichTextLabel cost;
    [Export] TextureRect affordable;

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

        UpdateCost(null, Core.Players.LocalPlayer.Gold);

        MLobby.MLobbyPlayerEvents.OnGoldAmountChanged += UpdateCost;
        TreeExiting += () => { MLobby.MLobbyPlayerEvents.OnGoldAmountChanged -= UpdateCost; };
        MouseEntered += () => { iMenu.selectedButton = actionIndex; frame.SelfModulate = Color.FromHtml("00ffff"); };
        MouseExited += () => { iMenu.selectedButton = -1; frame.SelfModulate = Colors.White; };
    }

    private void UpdateCost(object sender, int playerGold)
    {
        if (playerAction.ToolTip == "Sell")
        {
            cost.Text = "+" + playerAction.Cost.ToString();
            affordable.Hide();
        }
        else
        {
            cost.Text = playerAction.Cost.ToString();
            affordable.Modulate = playerGold >= playerAction.Cost ? Colors.Green : Colors.Red;
            affordable.Show();
        }
    }
}// EOF CLASS
