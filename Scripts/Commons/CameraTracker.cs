using Godot;
using OOTA.Enums;
using System;

namespace OOTA;

public partial class CameraTracker : Node3D
{
    [Export] Node3D target;
    [Export] Node3D avatarsContainer;
    [Export] bool freeLook = false;

    [ExportGroup("Camera Constraints")]
    [Export] float baseSpeed = 8.0f;
    [Export] float sideMargin = 8.0f;
    [Export] float topMargin = 2.0f;
    [Export] float bottomMargin = 2.0f;


    Vector3 inVec = Vector3.Zero;
    Vector3 Min => new Vector3(Core.Rules.MaxLeft + sideMargin, 0.0f, Core.Rules.MaxTop + topMargin);
    Vector3 Max => new Vector3(Core.Rules.MaxRight - sideMargin, 0.0f, Core.Rules.MaxBottom - bottomMargin);

    public override void _Ready()
    {
        LocalLogic.OnPlayerStateChanged += WhenPlayerStateChanged;
        Core.Camera = GetNode<Camera3D>("Camera3D");
    }

    private void WhenPlayerStateChanged(object sender, PLAYERSTATE newState)
    {
        //GD.Print($"CameraTracker::WhenPlayerStateChanged({newState})");
        if (newState == PLAYERSTATE.ALIVE)
        {
            LocalLogic.ShowHUD();
            target = Core.Players.LocalPlayer.Avatar;
            freeLook = false;
        }
        else
        {
            target = null;
            freeLook = true;
        }
    }


    public override void _Process(double delta)
    {
        if (freeLook) { DoFreeLook((float)delta); }
        if (target is null) { return; }
        if (Core.Players.LocalPlayer.State == PLAYERSTATE.ALIVE)
        {
            GlobalPosition = GlobalPosition.Lerp(target.GetGlobalTransformInterpolated().Origin, 0.5f);
            ClampCamera();
        }
    }

    private void DoFreeLook(float delta)
    {
        GD.Print($"CameraTracker::DoFreeLook()");

        // Building input vector Left stick
        inVec = Vector3.Zero;
        inVec += Vector3.Right * Input.GetAxis("LSLeft", "LSRight");
        inVec += Vector3.Back * Input.GetAxis("LSUp", "LSDown");

        GlobalPosition += inVec * baseSpeed * delta;
        ClampCamera();
    }

    private void ClampCamera()
    {
        GlobalPosition = GlobalPosition.Clamp(Min, Max);
    }
}// EOF CLASS
