using Godot;
using OOTA.Enums;
using OOTA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOTA.Units;

[GlobalClass]
public partial class UnitSupport : UnitBaseClass, ISupporter
{
    //[Export] float targetMinDistance = 3.0f;
    //[Export] float projectileSpeed = 6.0f;
    //[Export] float projectileTTL = 1.5f;

    [Export] float baseDamageBonus = 0.25f;
    [Export] float baseScaleBonus = 0.15f;


    public override void ProcessHunting(float delta)
    {
        inVec = Vector3.Zero;
        inVec = GlobalPosition.DirectionTo(target.GlobalPosition);
        // apply break if moving in wrong direction 
        if (LinearVelocity.Dot(inVec) < 0.5f)
        {
            LinearVelocity *= 0.5f;
        }
        // tweak velocity to stick harder on track
        float angle = LinearVelocity.SignedAngleTo(inVec, Vector3.Up);
        LinearVelocity = LinearVelocity.Rotated(Vector3.Up, angle);

        if (GlobalPosition.DistanceTo(target.GlobalPosition) > attackRange)
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
        else
        {
            // within range so slow down
            LinearVelocity *= 0.5f;
        }

    }

    public override void ProcessTraveling(float delta)
    {
        // if no path
        if (pathState == PATHSTATE.IDLE)
        {
            GetPathToTarget();
        }
        // Skip if waiting for path
        if (pathState == PATHSTATE.PENDING) { return; }

        // move along path
        if (pathState == PATHSTATE.EXECUTING)
        {
            //MGizmosCSharp.GizmoUtils.DrawLine(path, 5.0f, Colors.RebeccaPurple);

            // tweak velocity to stick harder on path
            float angle = LinearVelocity.SignedAngleTo(inVec, Vector3.Up);
            LinearVelocity = LinearVelocity.Rotated(Vector3.Up, angle);

            // Move along path
            inVec = Vector3.Zero;
            if (GlobalPosition.DistanceTo(nextPathPoint) < 0.2f) { pathIndex++; }

            if (pathIndex < path.Count)
            {
                //MGizmosCSharp.GizmoUtils.DrawLine(GlobalPosition, nextPathPoint, 0.1f, Colors.Red);

                inVec = GlobalPosition.DirectionTo(nextPathPoint);
                if (LinearVelocity.Dot(inVec) < 0.5f)
                {
                    LinearVelocity *= 0.5f;
                }

                if (inVec != Vector3.Zero)
                {
                    ApplyForce(inVec * Mass * acceleration * SpeedModifier());
                }
                else
                {
                    LinearVelocity *= 0.85f;
                }
            }
            else
            {
                // Arrived at end of path
                LinearVelocity *= 0.5f;
                pathState = PATHSTATE.FINISHED;
            }
        }
    }

    public override void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
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

    public override void BodyEnteredAggroRange(Node3D body)
    {
        if (body is UnitBaseClass unitEntering && unitEntering.Team == Team && unitEntering is not UnitSupport)
        {
            if (!targets.Contains(unitEntering))
            {
                targets.Add(unitEntering);
                unitEntering.AddSupporter(this);
                ResetMind();
            }
        }
    }
    public override void BodyExitedAggroRange(Node3D body)
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
                ResetMind();
            }
        }
    }
    private protected override void PickTarget()
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
        base.Die();
    }
}// EOF CLASS
