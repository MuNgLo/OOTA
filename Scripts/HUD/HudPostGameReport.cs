using Godot;
using MLobby;
using OOTA;
using OOTA.Enums;
using System;

namespace OOTA.HUD;

public partial class HudPostGameReport : PanelContainer
{
    [Export] Button btnContinue;
    public override void _Ready()
    {
        btnContinue.Pressed += WhenContinuePressed;
        MLobbyPlayerEvents.OnPlayersChanged += (s, o) => { if (Visible) { UpdateButtonText(); } };
        Core.Rules.gameStats.OnGameStateChanged += WhenGameStateChanged;
        MMenuSystem.HUDSystem.HideElement(Name);
    }

    private void WhenContinuePressed()
    {
        Core.Rules.PlayerRequestReady();
    }

    private void WhenGameStateChanged(object sender, GAMESTATE e)
    {
        if (e == GAMESTATE.POST)
        {
            UpdateButtonText();
            MMenuSystem.MenuSystem.HideMenu(true);
            MMenuSystem.HUDSystem.ShowElement(Name);
        }
        else
        {
            if (Visible)
            {
                MMenuSystem.HUDSystem.HideElement(Name);
                MMenuSystem.MenuSystem.ShowMenu();
            }
        }
    }

    private void UpdateButtonText()
    {
        btnContinue.Text = "CONTINUE " + Core.Players.ReadyTextForContinueButton;
    }
}// EOF CLASS
