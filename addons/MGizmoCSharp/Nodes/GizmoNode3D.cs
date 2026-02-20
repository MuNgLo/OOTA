using Godot;
namespace MGizmosCSharp;

[Tool, GlobalClass]
public partial class GizmoNode3D : Node3D
{
    [ExportGroup("Timing")]
    [Export] private GIZMOTIMING timing = GIZMOTIMING.FRAME;
    [Export] private float duration = 1.0f;

    private float timeToNextUpdate = 0.0f;

    public override void _Process(double delta)
    {
        if (timing == GIZMOTIMING.FRAME) { GizmoUpdate(); }
        if (timing == GIZMOTIMING.DURATION)
        {
            timeToNextUpdate -= (float)delta;
            if (timeToNextUpdate < 0.0f)
            {
                GizmoUpdate(duration);
                timeToNextUpdate = duration;
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (timing == GIZMOTIMING.PHYSICS) { GizmoUpdate((float)delta); }
    }
    public virtual void GizmoUpdate(float showFor = 0.0f)
    {
        GD.PrintErr($"GizmoNode3D::GizmoUpdate() Remember to override this in your custom gizmo node");
    }
}// EOF CLASS
