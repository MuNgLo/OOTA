using Godot;
using OOTA.Buildings;
using OOTA.Spawners;
using System;

namespace OOTA.Units;

[GlobalClass]
public partial class UnitRanged : UnitBaseClass
{
    [Export] float projectileSpeed = 6.0f;
    [Export] float projectileTTL = 1.5f;
    [Export] Node3D weaponMuzzle;

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

        if (target is Goal || GlobalPosition.DistanceTo(target.GlobalPosition) > attackRange)
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

        if (TargetInRange() && target is not Goal)
        {
            if (TargetIsToClose())
            {
                ApplyForce(-inVec * Mass * acceleration * SpeedModifier());
            }
            else
            {

                if (LinearVelocity != Vector3.Zero) { LinearVelocity *= 0.5f; }
                if (Time.GetTicksMsec() > lastAttack + attackCoolDownMS)
                {
                    lastAttack = Time.GetTicksMsec();
                    AttackTarget();
                }
            }
        }
    }


    public override void AttackTarget()
    {
        weaponMuzzle.LookAt(target.GlobalPosition + Vector3.Up * 0.25f, Vector3.Up);
        Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument("res://Scenes/Projectiles/Projectile.tscn",
            Team, BuildDamage(damage), projectileSpeed, projectileTTL, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
        ));
    }
}// EOF CLASS
