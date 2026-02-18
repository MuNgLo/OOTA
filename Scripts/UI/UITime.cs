using Godot;
using System;

public partial class UITime : RichTextLabel
{

    private TimeSpan timeSpan;
    public override void _Ready()
    {
        Text = "----";
        Core.Rules.OnGameStart += WhenGameStarts;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public override void _Process(double delta)
    {
        timeSpan = TimeSpan.FromSeconds(GameTimer.TotalGameTime);
        Text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}// EOF CLASS
