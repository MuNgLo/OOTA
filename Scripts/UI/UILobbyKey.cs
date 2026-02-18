using Godot;
using MLobby;
using System;

public partial class UILobbyKey : Control
{
    [Export] private LineEdit le_key;
    [Export] private Button btn_Copy;
    [Export] private TextureButton btn_Secret;

    public override void _Ready()
    {
        LobbyEvents.OnConnectedToServer += WhenConnectedToServer;
        LobbyEvents.OnHostSetupReady += WhenConnectedToServer;

        btn_Secret.Pressed += WhenSecretPressed;
        btn_Copy.Pressed += WhenCopyPressed;

        LobbyEvents.OnLeavingHost += (sender, args) => { Hide(); };
        LobbyEvents.OnHostClosed += (sender, args) => { Hide(); };
        LobbyEvents.OnServerDisconnected += (sender, args) => { Hide(); };
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

}// EOF CLASS
