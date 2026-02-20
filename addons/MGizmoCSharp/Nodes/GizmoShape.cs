using System.Collections.Generic;
using Godot;

namespace MGizmosCSharp;

[GlobalClass, Tool]
public partial class GizmoShape : GizmoNode3D
{
    [Export] private GSHAPES shape = GSHAPES.DIAMOND;
    [Export] private float shapeScale = 1.0f;
    [Export] private Color shapeColor = Colors.Yellow;
    [Export(PropertyHint.Range, "0,64")] private int shapeSubd = 12;


    public override void GizmoUpdate(float showFor = 0.0f)
    {
        if (shape == GSHAPES.CIRCLE)
        {
            GizmoUtils.DrawCircle(Transform, shapeSubd, showFor, shapeScale, shapeColor);
            return;
        }
        GizmoUtils.DrawShape(Transform, shape, showFor, shapeScale, shapeColor);
    }
}// EOF CLASS
