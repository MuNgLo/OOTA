using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[GlobalClass]
public partial class UnitBaseClass : RigidBody3D, ITargetable
{
    [Export] protected TEAM team = TEAM.NONE;
    public Node3D target;
    [Export] float moveSpeed = 8.0f;
    [Export] protected ulong attackCoolDownMS = 300;
    [Export] protected float acceleration = 80.0f;
    [Export] protected int damage = 3;
    [Export] protected float attackRange = 1.0f;
    [Export] protected float range = 6.0f;
    [Export] float targetMinDistance = 3.0f;



    [Export] float health = 100;
    [Export] float maxHealth = 100;
    [Export] protected Area3D area;

    public double NormalizedHealth => Math.Clamp(health / maxHealth, 0.0, 1.0);

    protected Vector3 inVec;
    protected List<ITargetable> targets;
    protected List<ISupporter> supporters;
    protected ulong lastAttack;
    public TEAM Team { get => team; set => SetTeam(value); }
    public Node3D Body => this;

    public virtual void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
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
        PickTarget();

        inVec = Vector3.Zero;
        inVec = GlobalPosition.DirectionTo(target.GlobalPosition);

        if (LinearVelocity.Dot(inVec) < 0.5f)
        {
            LinearVelocity *= 0.5f;
        }

        if (TargetInRange())
        {
            if (LinearVelocity != Vector3.Zero) { LinearVelocity *= 0.5f; }
            if (Time.GetTicksMsec() > lastAttack + attackCoolDownMS)
            {
                lastAttack = Time.GetTicksMsec();
                AttackTarget();
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

    private protected bool TargetInRange()
    {
        if (target is Base baseBuilding)
        {
            return GlobalPosition.DistanceTo(target.GlobalPosition) <= attackRange + 2.8f;
        }
        if (target is Base unit)
        {
            return GlobalPosition.DistanceTo(target.GlobalPosition) <= attackRange + 0.5f;
        }
        return GlobalPosition.DistanceTo(target.GlobalPosition) <= attackRange;
    }

    public virtual void AttackTarget()
    {
        if (target is UnitBaseClass unit) { unit.TakeDamage(BuildDamage(damage)); return; }
        if (target is BuildingBaseClass building) { building.TakeDamage(BuildDamage(damage)); return; }
        if (target is PlayerAvatar avatar) { avatar.TakeDamage(BuildDamage(damage)); return; }
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

    public void AddHealth(int amount)
    {
        if (amount < 1) { return; }
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount < 1) { return; }
        health -= amount;
        if (health <= 0.0f) { Die(); }
    }

    public virtual void Die()
    {
        Core.UnitDied(this);
    }

    private protected float SpeedModifier()
    {
        if (LinearVelocity != Vector3.Zero)
        {
            return 1.0f - LinearVelocity.Length() / moveSpeed;
        }
        return 1.0f;
    }

    public virtual void WhenBodyEntered(Node3D body)
    {
        if (body is ITargetable t && t.Team != Team)
        {
            if (!targets.Contains(t))
            {
                targets.Add(t);
            }
        }
    }
    public virtual void WhenBodyExited(Node3D body)
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
    public virtual void PickTarget()
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
        TargetEnemyBase();
    }

    private protected void SetTarget(ITargetable targetable)
    {
        if (targetable is UnitBaseClass) { target = targetable as UnitBaseClass; return; }
        if (targetable is BuildingBaseClass) { target = targetable as StaticBody3D; return; }
        if (targetable is PlayerAvatar) { target = targetable as RigidBody3D; return; }
    }

    public void TargetEnemyBase()
    {
        target = team == TEAM.LEFT ? Core.Rules.RightBase : Core.Rules.LeftBase;
    }
    public void TargetFriendlyBase()
    {
        target = team == TEAM.LEFT ? Core.Rules.LeftBase : Core.Rules.RightBase;
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
        if(!IsInstanceValid(this) || !Multiplayer.HasMultiplayerPeer()){return;}
        float totalScale = 1.0f;
        foreach (ISupporter supporter in supporters)
        {
            totalScale += supporter.BaseScaleBonus();
        }
        Rpc(nameof(RPCSetUnitScale), totalScale);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode =  MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCSetUnitScale(float totalScale)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Scale = Vector3.One * totalScale;
        GetNode<CollisionShape3D>("CollisionShape3D").Scale = Vector3.One * totalScale;
    }
    public void RemoveSupporter(ISupporter supporter)
    {
        if (supporters.Exists(p => p.GetInstanceId() == supporter.GetInstanceId()))
        {
            supporters.RemoveAll(p => p.GetInstanceId() == supporter.GetInstanceId());
        }
        UpdateUnitScale();
    }


    private protected bool TargetIsToClose()
    {
        return GlobalPosition.DistanceTo(target.GlobalPosition) < targetMinDistance;
    }
}// EOF CLASS