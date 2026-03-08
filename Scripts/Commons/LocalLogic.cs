using Godot;
using MConsole;
using MLobby;
using MMenuSystem;
using OOTA.Enums;
using OOTA.UI;
using System;
using System.Collections.Generic;

namespace OOTA;

public partial class LocalLogic : Node
{
    private static LocalLogic ins;
    [Export] LobbyManager lobbyManager;
    [Export] Node3D avatarContainer;
    public static UIMainMenu mainMenu;

    public override void _EnterTree()
    {
        ins = this;
    }


    public override void _Ready()
    {
        lobbyManager.ChildEnteredTree += WhenLobbyMemberAdded;
        avatarContainer.ChildEnteredTree += WhenAvatarAdded;
        Core.Rules.OnGameStart += WhenGameStarts;
        RegisterConsoleCommands();
        OnPlayerStateChanged += WhenPlayerStateChanged;
    }

    private void WhenAvatarAdded(Node node)
    {
        if (Core.Players.GetPlayer(node.GetMultiplayerAuthority(), out OOTAPlayer player)) { player.Avatar = node as PlayerAvatar; }
    }

    private void WhenPlayerStateChanged(object sender, PLAYERSTATE e)
    {
        if (e == PLAYERSTATE.DEAD)
        {
            Node ava = Core.Players.LocalPlayer.Avatar;
            MultiplayerSynchronizer synchronizer = ava.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
            synchronizer.PublicVisibility = false;
            synchronizer.UpdateVisibility();
            ava.SetMultiplayerAuthority(1);
            Core.Rules.PlayerRequestHandOverAvatar(ava.GetPath());
        }
    }

    private void RegisterConsoleCommands()
    {
        Command cmd = new Command("RegisterDefaultCommands", -1)
        {
            Name = "name",
            Tip = "Set your player name",
            Help = "Example: name Bob",
            act = (a) =>
            {

                Core.Rules.PlayerRequestNameChange(a[1]);
                return "Sending name change request to host";
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
        MenuSystem.HideMenu();
    }



    private void WhenLobbyMemberAdded(Node node)
    {

        if (node is LobbyMember member)
        {
            //GD.Print($"LocalLogic::WhenLobbyMemberAdded() {member.PeerID}");
            if (member.PeerID == Multiplayer.GetUniqueId())
            {
                mainMenu.ConnectedToGame();
            }
        }
    }

    public static void ShowHUD()
    {
        HUDSystem.HideAll();
        HUDSystem.ShowElements([
            "HUDWorldElements",
            "Resources",
            "HealthBar",
            "TopElement"
        ], true);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            MMenuSystem.MenuSystem.ToggleMenu();
        }
        if (Input.IsActionJustPressed("ToggleConsole"))
        {
            MConsole.GameConsole.Toggle();
        }

    }

    //public static event EventHandler<PlayerAvatar> OnAvatarAssigned;

    /*internal static void AvatarChanged(PlayerAvatar avatar)
    {
        OnAvatarAssigned?.Invoke(null, avatar);
    }*/

    #region Local Events from PlayerData

    public static event EventHandler<PLAYERMODE> OnPlayerModeChanged;

    internal static void PlayerModeChanged(PLAYERMODE mode)
    {
        OnPlayerModeChanged?.Invoke(null, mode);
    }

    public static event EventHandler<PLAYERSTATE> OnPlayerStateChanged;

    internal static void PlayerStateChanged(PLAYERSTATE state)
    {
        OnPlayerStateChanged?.Invoke(null, state);
    }
    public static event EventHandler<bool> OnReadyChanged;

    internal static void ReadyChanged(bool value)
    {
        OnReadyChanged?.Invoke(null, value);
    }
    public static event EventHandler<TEAM> OnTeamChanged;
    internal static void TeamChanged(TEAM team)
    {
        OnTeamChanged?.Invoke(null, team);
    }
    /// <summary>
    /// Carries currentHealth,max health
    /// </summary>
    public static event EventHandler<float[]> OnHealthChanged;

    internal static void HealthChanged(float[] values)
    {
        OnHealthChanged?.Invoke(null, values);
    }


    public static event EventHandler<List<PlayerActionStruct>> OnHudInteractMenu;

    /// <summary>
    /// If sender is Null the interact menu will hide.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="interactions"></param>
    internal static void RaiseHudInteractMenu(Object sender, List<PlayerActionStruct> interactions)
    {
        OnHudInteractMenu?.Invoke(sender, interactions);
    }


    #endregion
}// EOF CLASS
