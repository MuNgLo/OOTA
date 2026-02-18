using System;
using Godot;

namespace HUD;

public class HUDWorldLabel
{
    public readonly ulong id;
    public Vector3 point;

    public Control element;

    private RichTextLabel richText => element as RichTextLabel;
    public string Text { get => richText.Text; set => richText.Text = value; }

    private ProgressBar bar => element as ProgressBar;

    public double Value { get => bar.Value; set => bar.Value = value; }
    public Color Color { get => (bar.GetThemeStylebox("fill", "ProgressBar") as StyleBoxFlat).BgColor; set => SetBGColor(value); }


 private StyleBoxFlat fillStyle;


    private void SetBGColor(Color value)
    {
        //(bar.GetThemeStylebox("fill", "ProgressBar") as StyleBoxFlat).BgColor = value;
        fillStyle.BgColor = value;
    }


    public HUDWorldLabel(ulong id)
    {
        this.id = id;
        element = new RichTextLabel()
        {
            BbcodeEnabled = true,
            Size = new Vector2(200.0f, 30.0f),
            FitContent = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            PivotOffset = new Vector2(-100.0f, -15.0f),
        };
        element.Name = id.ToString();
        element.Scale = 0.75f * Vector2.One;
    }
    public HUDWorldLabel(ulong id, ProgressBar pBar, double normalizedHealth)
    {
        this.id = id;

        fillStyle = new StyleBoxFlat() {BgColor = Colors.Pink };
        
        fillStyle.SetBorderWidthAll(1);
        fillStyle.BorderColor = Colors.Black;
        
        pBar.AddThemeStyleboxOverride("fill", fillStyle);

        element = pBar;
        pBar.MaxValue = 1.0f;
        pBar.MinValue = 0.0f;
        pBar.Step = 0.01f;
        pBar.Value = normalizedHealth;
    }
}// EOF CLASS