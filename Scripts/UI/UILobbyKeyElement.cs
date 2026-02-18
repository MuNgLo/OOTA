using Godot;
using MLobby;
using System;

namespace UIAPI.Menus;

public partial class UILobbyKeyElement : Control
{
    [Export] private LineEdit le_key;
    [Export] private Button btn_Copy;
    [Export] private TextureButton btn_Secret;


    public override void _Ready()
    {
        Core.Lobby.LobbyEvents.OnConnectedToServer += WhenConnectedToServer;
        Core.Lobby.LobbyEvents.OnHostSetupReady += WhenConnectedToServer;

        MMenuSystem.MenuSystem.OnMenuVisibilityChanged += (s, o) => { Visible = o && le_key.Text != string.Empty; };

        btn_Secret.Pressed += WhenSecretPressed;
        btn_Copy.Pressed += WhenCopyPressed;

        Core.Lobby.LobbyEvents.OnLeavingHost += (sender, args) => { Hide(); };
        Core.Lobby.LobbyEvents.OnHostClosed += (sender, args) => { Hide(); };
        Core.Lobby.LobbyEvents.OnServerDisconnected += (sender, args) => { Hide(); };
        le_key.Secret = true;
        Hide();
    }

    private void WhenConnectedToServer(object sender, ConnectedEventArguments e)
    {
        string key = Core.AddressAndPortToString(e.ip, e.port);
        le_key.Text = key;
        Show();
    }

    public void WhenSecretPressed()
    {
        le_key.Secret = !le_key.Secret;
    }
    private void WhenCopyPressed()
    {
        DisplayServer.ClipboardSet(le_key.Text);
    }

}// EOF  CLASS
