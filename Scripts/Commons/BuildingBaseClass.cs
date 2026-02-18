using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BuildingBaseClass : StaticBody3D, ITargetable
{
    [Export] protected TEAM team = TEAM.NONE;
    [Export] protected float health = 5;
    [Export] protected float maxHealth = 100;


    [ExportGroup("Attack")]
    [Export] protected float range = 5.0f;
    [Export] protected float projectileSpeed = 8.0f;
    [Export] protected float projectileTTL = 1.5f;
    [Export] protected Node3D weaponPivot;
    [Export] protected Node3D weaponMuzzle;
    [Export] protected int damage = 5;
    [Export] protected ulong attackCoolDownMS = 450;

    [Export] protected Area3D area;
    protected List<ITargetable> targets;
    protected ulong tsLastAttackMS;
    protected Node3D target;
    protected List<ISupporter> supporters;

    public TEAM Team { get => team; set => SetTeam(value); }
    public double NormalizedHealth => Math.Clamp(health / maxHealth, 0.0, 1.0);
    public Node3D Body => this;


    private void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("TeamMesh").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        area.CollisionMask = value == TEAM.RIGHT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }



    public override void _Ready()
    {
        targets = new List<ITargetable>();
        supporters = new List<ISupporter>();
        if (Multiplayer.IsServer())
        {
            health = maxHealth;
            if (area is null) { GD.PushError($"Unit don't have an area3D [{GetPath()}]"); }
            area.BodyEntered += WhenBodyEntered;
            area.BodyExited += WhenBodyExited;
            (area.GetNode<CollisionShape3D>("CollisionShape3D").Shape as CylinderShape3D).Radius = range;
        }
        else
        {
            area.Monitoring = false;
            area.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer()) { return; }
        if (Time.GetTicksMsec() > tsLastAttackMS + attackCoolDownMS)
        {
            PickTarget();
            if (target is not null)
            {
                if (GlobalPosition.DistanceTo(target.GlobalPosition) <= range)
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
        Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument(
            Team, BuildDamage(damage), projectileSpeed, projectileTTL, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
        ));
    }



    private protected virtual void PickTarget()
    {
        if (targets.Count > 0)
        {
            List<ITargetable> candidates = targets.FindAll(p => p.Team != team).OrderBy(p => p.GlobalPosition.DistanceTo(GlobalPosition)).ToList();
            if (candidates.Count > 0)
            {
                SetTarget(candidates[0]);
                return;
            }
        }
        target = null;
    }

    public virtual void AddHealth(int amount)
    {
        if (amount < 1) { return; }
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount < 1) { return; }
        health -= amount;
        if (health <= 0.0f) { Die(); }
    }

    private protected virtual void WhenBodyEntered(Node3D body)
    {
        if (body is ITargetable t && t.Team != team)
        {
            if (!targets.Contains(t))
            {
                targets.Add(t);
            }
        }
    }
    private protected virtual void WhenBodyExited(Node3D body)
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
    private protected virtual void Die()
    {
        Core.BuildingDied(this);
    }
    private protected void SetTarget(ITargetable targetable)
    {
        if (targetable is UnitBaseClass) { target = targetable as UnitBaseClass; return; }
        if (targetable is BuildingBaseClass) { target = targetable as StaticBody3D; return; }
        if (targetable is PlayerAvatar) { target = targetable as RigidBody3D; return; }
    }

    public void AddSupporter(ISupporter supporter)
    {
        if (!supporters.Exists(p => p.GetInstanceId() == supporter.GetInstanceId()))
        {
            supporters.Add(supporter);
            supporter.TreeExiting += () => { RemoveSupporter(supporter); };
            UpdateUnitScale();
        }
    }

    private void UpdateUnitScale()
    {
        float totalScale = 1.0f;
        foreach (ISupporter supporter in supporters)
        {
            totalScale += supporter.BaseScaleBonus();
        }
        GetNode<MeshInstance3D>("MeshInstance3D").Scale = Vector3.One * totalScale;
    }

    public void RemoveSupporter(ISupporter supporter)
    {
        if (supporters.Exists(p => p.GetInstanceId() == supporter.GetInstanceId()))
        {
            supporters.RemoveAll(p => p.GetInstanceId() == supporter.GetInstanceId());
        }
        UpdateUnitScale();
    }


    private protected int BuildDamage(int damage)
    {
        int extraDamage = 0;

        foreach (ISupporter supporter in supporters)
        {
            extraDamage += supporter.BaseDamageBonus(damage);
        }
        return damage + extraDamage;
    }
}// EOF CLASS



