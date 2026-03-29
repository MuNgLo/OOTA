using Godot;
using MSettings;
namespace OOTA.UI;

public partial class UIEnableVIOPMic : CheckButton
{
	GameConfigSettings Config => Settings.GetCachedSettings("GameConfigSettings") as GameConfigSettings;
	public override void _Ready()
	{
		Toggled += WhenToggled;
		Settings.OnSettingsChange += WhenSettingsChange;
		SetPressedNoSignal(Config.voipMicEnabled);
		WhenToggled(Config.voipMicEnabled);
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if (e is GameConfigSettings)
		{
			SetPressedNoSignal(Config.voipMicEnabled);
		}
	}

	private void WhenToggled(bool toggledOn)
	{
		Config.voipMicEnabled = toggledOn;
		Settings.SaveSettings(Config, "Configs", true);
		//Turn the input device on/off
		if (Config.voipMicEnabled)
		{
			Error err = AudioServer.SetInputDeviceActive(true);
			if (err != Error.Ok)
			{
				MLogging.MLog.LogError($"Mic input err -> [{err}]", true);
			}
		}
		else
		{
			AudioServer.SetInputDeviceActive(false);
		}
	}


}// EOF CLASS
