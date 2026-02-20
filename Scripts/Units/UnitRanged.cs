using Godot;
using System;
[GlobalClass]
public partial class UnitRanged : UnitBaseClass
{
    [Export] float projectileSpeed = 6.0f;
    [Export] float projectileTTL = 1.5f;
    [Export] Node3D weaponMuzzle;
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
                if (Time.GetTicksMsec() > lastAttack + attackCoolDownMS)
                {
                    lastAttack = Time.GetTicksMsec();
                    AttackTarget();
                }
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

  

    public override void AttackTarget()
    {
        weaponMuzzle.LookAt(target.GlobalPosition + Vector3.Up * 0.25f, Vector3.Up);
        Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument("res://Scenes/Projectiles/Projectile.tscn",
            Team, BuildDamage(damage), projectileSpeed, projectileTTL, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
        ));
    }
}// EOF CLASS
