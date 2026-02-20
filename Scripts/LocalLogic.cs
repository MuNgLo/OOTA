using Godot;
using MConsole;
using MLobby;
using MMenuSystem;
using System;
using UI.Menus;

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
        Core.Rules.OnGameStart += WhenGameStarts;
        RegisterConsoleCommands();
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

    public static event EventHandler<PlayerAvatar> OnAvatarAssigned;

    internal static void AvatarChanged(PlayerAvatar avatar)
    {
        OnAvatarAssigned?.Invoke(null, avatar);
    }

    #region Local Events from PlayerData

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
    #endregion
}// EOF CLASS
