using Godot;
using System;

public partial class UILobbyKey : Control
{
    [Export] private LineEdit le_key;
    [Export] private Button btn_Copy;
    [Export] private TextureButton btn_Secret;

    public override void _Ready()
    {
        Core.OnLobbyKeyActive += WhenLobbyKeyActive;

        btn_Secret.Pressed += WhenSecretPressed;
        btn_Copy.Pressed += WhenCopyPressed;

        Core.Lobby.LobbyEvents.OnLeavingHost += (sender, args) => { Hide(); };
        Core.Lobby.LobbyEvents.OnHostClosed += (sender, args) => { Hide(); };
        Core.Lobby.LobbyEvents.OnServerDisconnected += (sender, args) => { Hide(); };
        le_key.Secret = true;
        Hide();
    }

    public void WhenSecretPressed()
    {
        le_key.Secret = !le_key.Secret;
    }
    private void WhenCopyPressed()
    {
        DisplayServer.ClipboardSet(le_key.Text);
    }

    private void WhenLobbyKeyActive(object sender, string e)
    {
        le_key.Text = e;
        Show();
    }

}// EOF CLASS
