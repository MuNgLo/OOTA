using Godot;
using OOTA.Enums;
using OOTA.Interfaces;
using System;

namespace OOTA.Components;

public partial class AwarenessArea3D : Area3D
{
    [Export] float radius = 5.0f;
    [Export] bool flipCollisionMask = false;
    [Export] CollisionShape3D collisionShape;
    IMind mind;

    CylinderShape3D Shape => collisionShape.Shape as CylinderShape3D;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            if (GetParent() is IMind m)
            {
                mind = m;
            }
            else
            {
                GD.PushError($"AwarenessArea3D parent don't have a IMind [{GetPath()}]");
            }
            BodyEntered += mind.BodyEnteredAggroRange;
            BodyExited += mind.BodyExitedAggroRange;
            Shape.Radius = radius;
            if(flipCollisionMask)
            {
                CollisionMask = mind.Team == TEAM.RIGHT ? Core.Rules.rightTeamCollision : Core.Rules.leftTeamCollision;
            }
            else
            {
                CollisionMask = mind.Team == TEAM.RIGHT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
            }
        }
        else
        {
            Monitoring = false;
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }


}// EOF CLASS
