using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[GlobalClass]
public partial class UnitSupport : UnitBaseClass, ISupporter
{
    //[Export] float targetMinDistance = 3.0f;
    //[Export] float projectileSpeed = 6.0f;
    //[Export] float projectileTTL = 1.5f;

    [Export] float baseDamageBonus = 0.25f;
    [Export] float baseScaleBonus = 0.15f;

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer()) { return; }
        PickTarget();

        inVec = Vector3.Zero;
        inVec = GlobalPosition.DirectionTo(target.GlobalPosition);

        if (LinearVelocity.Dot(inVec) < 0.5f)
        {
            LinearVelocity *= 0.5f;
        }

        if (TargetInRange())
        {
            if (TargetIsToClose())
            {
                ApplyForce(-inVec * Mass * acceleration * SpeedModifier());
            }
            else
            {
                if (LinearVelocity != Vector3.Zero) { LinearVelocity *= 0.5f; }
            }
        }
        else
        {
            if (inVec != Vector3.Zero)
            {
                ApplyForce(inVec * Mass * acceleration * SpeedModifier());
            }
            else
            {
                LinearVelocity *= 0.85f;
            }
        }
    }


    public override void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        area.CollisionMask = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }

    public int BaseDamageBonus(int originalDamage)
    {
        return Mathf.FloorToInt(originalDamage * baseDamageBonus);
    }

    public float BaseScaleBonus()
    {
        return baseScaleBonus;
    }

    public int ExtraHealth(int currentMaxHealth)
    {
        throw new NotImplementedException();
    }

    public override void WhenBodyEntered(Node3D body)
    {
        if (body is UnitBaseClass unitEntering && unitEntering.Team == Team && unitEntering is not UnitSupport)
        {
            if (!targets.Contains(unitEntering))
            {
                targets.Add(unitEntering);
                unitEntering.AddSupporter(this);
            }
        }
    }
    public override void WhenBodyExited(Node3D body)
    {
        if (body is ITargetable targetable)
        {
            if (targets.Contains(targetable))
            {
                targets.Remove(targetable);
                targetable.RemoveSupporter(this);
            }
            if (target is not null && target == body)
            {
                PickTarget();
            }
        }
    }
    public override void PickTarget()
    {
        if (targets.Count > 0)
        {
            List<ITargetable> candidates = targets.OrderBy(p => p.GlobalPosition.DistanceTo(GlobalPosition)).ToList();
            if (candidates.Count > 0)
            {
                SetTarget(candidates[0]);
                return;
            }
        }
        TargetFriendlyBase();
    }

    public override void Die()
    {
        foreach (ITargetable targetable in targets)
        {
            targetable.RemoveSupporter(this);
        }
        Core.UnitDied(this);
    }
}// EOF CLASS
