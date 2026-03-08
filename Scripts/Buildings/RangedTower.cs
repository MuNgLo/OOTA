using Godot;
using System;
using OOTA.Interfaces;
using OOTA.Spawners;
using System.Threading.Tasks;
namespace OOTA.Buildings;

public partial class RangedTower : BuildingBaseClass, IMind
{

    [ExportGroup("Attack")]
    [Export] protected float attackRange = 5.0f;
    [Export] protected bool canAttackBuildings = false;
    [Export] protected int baseDamage = 5;
    [Export] protected ulong attackCoolDownMS = 450;

    [ExportGroup("Projectile")]
    [Export] PackedScene prefabProjectile;
    [Export] protected float projectileSpeed = 8.0f;
    [Export] protected float projectileTTL = 1.5f;

    [ExportGroup("References")]
    [Export] protected Node3D weaponPivot;
    [Export] protected Node3D weaponMuzzle;


    public event EventHandler<float> OnAttack;
    public event EventHandler<float> OnReload;

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer()) { return; }
        if (Time.GetTicksMsec() > tsSpawnMS + attackCoolDownMS)
        {
            PickTarget();
            if (target is not null)
            {
                if (GlobalPosition.DistanceTo(target.GlobalPosition) <= attackRange)
                {
                    tsSpawnMS = Time.GetTicksMsec();
                    AttackTarget();
                }
            }
        }
    }

    public virtual async void AttackTarget()
    {
        weaponPivot.LookAt(target.GlobalPosition + Vector3.Up * 0.25f, Vector3.Up);


        OnAttack?.Invoke(this, 1000.0f / attackCoolDownMS * 5.0f);
        Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument(prefabProjectile.ResourcePath,
            Team, BuildDamage(baseDamage), projectileSpeed, projectileTTL, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
        ));

        await Task.Delay(Mathf.FloorToInt(1000.0f / attackCoolDownMS * 5.0f));
        OnReload?.Invoke(this, 1000.0f / attackCoolDownMS);
    }

    public void BodyEnteredAggroRange(Node3D body)
    {
        if (body is ITargetable t && t.Team != team)
        {
            if (t is BuildingBaseClass && canAttackBuildings == false)
            {
                return;
            }
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
