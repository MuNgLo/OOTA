using System;
using Godot;
using MSettings;
using OOTA.UI;

namespace OOTA;

public partial class SettingsEnforcer : Node
{

	GameConfigSettings Config => Settings.GetCachedSettings("GameConfigSettings") as GameConfigSettings;

	public override void _Ready()
	{
		Settings.OnSettingsChange += WhenSettingsChange;
	}

	public override void _Notification(int what)
	{
		// Moute on lost focus untested
		if (what == MainLoop.NotificationApplicationFocusIn)
		{
			AudioServer.SetBusMute(0, false);
		}
		if (what == MainLoop.NotificationApplicationFocusOut && Config.muteOnFocusLoss)
		{
			AudioServer.SetBusMute(0, true);
		}
		base._Notification(what);
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if (e is GameConfigSettings config)
		{
			SetVolumes(config);
			SetPlayerName();
		}
	}

	private void SetPlayerName()
	{
		// If it did. check if there is a network and game
		if (Multiplayer.HasMultiplayerPeer() && Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected)
		{
			// Check if the name differs from the playerData
			if (UISettingsMenu.PlayerNameLineEdit.Text != Core.Players.LocalPlayer.PlayerName)
			{
				// if it does, treat the change as a name change request
				Core.Rules.PlayerRequestNameChange(UISettingsMenu.PlayerNameLineEdit.Text);
			}
		}
		else
		{
			// No network so just set the lineEdit to the config value
			UISettingsMenu.PlayerNameLineEdit.Text = Config.playerName;
		}
	}

	private void SetVolumes(GameConfigSettings config)
	{
		// volume master
		AudioServer.SetBusVolumeLinear(0, config.volumeMaster);

		// volume Voices
		AudioServer.SetBusVolumeLinear(1, config.volumeVoices);
	}
}// EOF CLASS
