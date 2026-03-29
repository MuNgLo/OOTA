using Godot;
using MSettings;
using OOTA;
namespace OOTA.UI;

public partial class UIVOIPDeNoise : CheckButton
{
	GameConfigSettings Config => Settings.GetCachedSettings("GameConfigSettings") as GameConfigSettings;
	public override void _Ready()
	{
		Toggled += WhenToggled;
		Settings.OnSettingsChange += WhenSettingsChange;
		SetPressedNoSignal(Config.voipDeNoise);
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if (e is GameConfigSettings)
		{
			SetPressedNoSignal(Config.voipDeNoise);
			Node node = GetTree().Root.GetNode("Main/Core/MicIn");
			GetTree().Root.GetNode("Main/Core/MicIn").Call("SetDeNoise", Config.voipDeNoise);
		}
	}

	private void WhenToggled(bool toggledOn)
	{
		Config.voipDeNoise = toggledOn;
		Settings.SaveSettings(Config, "Configs");
	}



}// EOF CLASS

