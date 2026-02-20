using Godot;

namespace MGizmosCSharp;

[GlobalClass, Tool]
public partial class GizmoLine : GizmoNode3D
{
    [Export] private Color col = Colors.Yellow;
    [Export] private Node3D start;
    [Export] private Node3D end;

    public override void GizmoUpdate(float showFor = 0.0f)
    {
        if (start is not null && end is not null)
        {
            GizmoUtils.DrawLine(start.GlobalPosition, end.GlobalPosition, showFor, col);
        }
    }
}// EOF CLASS