using System;
using Godot;
using MSettings;
namespace OOTA.UI;
public partial class UIVOIPMicMode : HFlowContainer
{
	[Export] Button btnPTT;
	[Export] Button btnVoice;
	GameConfigSettings Config => Settings.GetCachedSettings("GameConfigSettings") as GameConfigSettings;
	
	public override void _Ready()
	{
		btnPTT.ToggleMode = true;
		btnVoice.ToggleMode = true;
		btnPTT.Toggled += WhenPTTPressed;
		btnVoice.Toggled += WhenVoicePressed;
		Settings.OnSettingsChange += WhenSettingsChange;
		UpdateButtonStates();
	}
	private void UpdateButtonStates()
	{
		btnPTT.SetPressedNoSignal(Config.voipMode);
		btnVoice.SetPressedNoSignal(!Config.voipMode);
	}
	private void WhenVoicePressed(bool toggledOn)
	{
		Config.voipMode = !toggledOn;
		Settings.SaveSettings(Config, "Configs");
	}

	private void WhenPTTPressed(bool toggledOn)
	{
		Config.voipMode = toggledOn;
		Settings.SaveSettings(Config, "Configs");
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if (e is GameConfigSettings)
		{
			UpdateButtonStates();
		}
	}

	
}// EOF CLASS
