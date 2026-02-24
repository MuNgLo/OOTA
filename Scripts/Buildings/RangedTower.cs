using Godot;
using System;
using OOTA.Interfaces;
using OOTA.Spawners;
namespace OOTA.Buildings;

public partial class RangedTower : BuildingBaseClass, IMind
{

    [ExportGroup("Attack")]
    [Export] protected float attackRange = 5.0f;
    [Export] protected int baseDamage = 5;
    [Export] protected ulong attackCoolDownMS = 450;

    [ExportGroup("Projectile")]
    [Export] PackedScene prefabProjectile;
    [Export] protected float projectileSpeed = 8.0f;
    [Export] protected float projectileTTL = 1.5f;

    [ExportGroup("References")]
    [Export] protected Node3D weaponPivot;
    [Export] protected Node3D weaponMuzzle;

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer()) { return; }
        if (Time.GetTicksMsec() > tsLastAttackMS + attackCoolDownMS)
        {
            PickTarget();
            if (target is not null)
            {
                if (GlobalPosition.DistanceTo(target.GlobalPosition) <= attackRange)
                {
                    tsLastAttackMS = Time.GetTicksMsec();
                    AttackTarget();
                }
            }
        }
    }

    public virtual void AttackTarget()
    {
        weaponPivot.LookAt(target.GlobalPosition + Vector3.Up * 0.25f, Vector3.Up);
        Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument(prefabProjectile.ResourcePath,
            Team, BuildDamage(baseDamage), projectileSpeed, projectileTTL, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
        ));
    }

    public void BodyEnteredAggroRange(Node3D body)
    {
        if (body is ITargetable t && t.Team != team)
        {
            if (!targets.Contains(t))
            {
                targets.Add(t);
            }
        }
    }
    public void BodyExitedAggroRange(Node3D body)
    {
        if (body is ITargetable t)
        {
            if (targets.Contains(t))
            {
                targets.Remove(t);
            }
            if (target is not null && target == body)
            {
                PickTarget();
            }
        }
    }
}// EOF CLASS
