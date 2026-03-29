using Godot;
using MSettings;
namespace OOTA.UI;
public partial class UIEnableVIOPButton : CheckButton
{
	GameConfigSettings Config => Settings.GetCachedSettings("GameConfigSettings") as GameConfigSettings;
	public override void _Ready()
	{
		Toggled += WhenToggled;
		Settings.OnSettingsChange += WhenSettingsChange;
		SetPressedNoSignal(Config.voipEnabled);
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if(e is GameConfigSettings)
		{
			SetPressedNoSignal(Config.voipEnabled);
		}
	}

	private void WhenToggled(bool toggledOn)
	{
		Config.voipEnabled = toggledOn;
		Settings.SaveSettings(Config, "Configs", true);
	}
}// EOF CLASS
