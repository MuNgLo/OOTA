using Godot;
using System;

namespace OOTA;

public partial class CameraTracker : Node3D
{
    [Export] Node3D target;
    [Export] Node3D avatarsContainer;


    public override void _Ready()
    {
        Core.Rules.OnGameStart += WhenGameStarts;
        LocalLogic.OnAvatarAssigned += WhenAvatarAssigned;
        Core.Camera = GetNode<Camera3D>("Camera3D");
    }

    private void WhenAvatarAssigned(object sender, PlayerAvatar playerAvatar)
    {
        LocalLogic.ShowHUD();
        target = playerAvatar;
        if(target is not null)
        {
            target.TreeExiting += () => { target = null; };
        }
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
    }

    public override void _Process(double delta)
    {
        if(target is null){ return; }
        GlobalPosition = GlobalPosition.Lerp(target.GetGlobalTransformInterpolated().Origin, 0.5f);
    }
}// EOF CLASS
