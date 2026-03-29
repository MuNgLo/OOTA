using Godot;
using System;
using MSettings;

namespace OOTA.UI;

public partial class UISettingsMenu : Control
{
	private static UISettingsMenu ins;
	[Export] float transitionDuration = 0.5f;
	[Export] Button btnCog;
	[Export] LineEdit playerName;

	public static LineEdit PlayerNameLineEdit => ins.playerName;

	bool isShowing = false;
	Vector2 ogPos;
	Vector2 LeftPos => ogPos - Vector2.Right * Size.X * 0.98f;

	public override void _EnterTree()
	{
		ins = this;
	}

	public override void _Ready()
	{
		ogPos = Position;
		btnCog.Pressed += WhenCogPressed;
		Settings.OnSettingsChange += WhenSettingsChange;
	}

	private void WhenSettingsChange(object sender, object e)
	{
		if(e is GameConfigSettings config)
		{
			Settings.SaveSettings<GameConfigSettings>(config ,"Configs", true);
		}
	}

	private void WhenCogPressed()
	{
		if (isShowing)
		{
			MoveIn();
		}
		else
		{
			MoveOut();
		}
	}

	private void MoveIn()
	{
		isShowing = false;
		var tween = CreateTween();
		tween.TweenProperty(this, "position", ogPos, transitionDuration)
			  .SetTrans(Tween.TransitionType.Linear)
			  .SetEase(Tween.EaseType.InOut);
	}

	private void MoveOut()
	{
		isShowing = true;
		var tween = CreateTween();
		tween.TweenProperty(this, "position", LeftPos, transitionDuration)
			  .SetTrans(Tween.TransitionType.Linear)
			  .SetEase(Tween.EaseType.InOut);
	}


}// EOF CLASS
