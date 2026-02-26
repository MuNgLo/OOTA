using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using OOTA.Interfaces;
using OOTA.Units;
using OOTA.Enums;

namespace OOTA.Buildings;
public partial class BuildingBaseClass : StaticBody3D, ITargetable
{
    [Export] protected TEAM team = TEAM.NONE;
    [Export] public TOWERTYPE towerType = TOWERTYPE.NONE;

    [ExportGroup("Stats")]
    [Export] public bool canTakeDamage = true;
    [Export] protected float aggroRange = 5.0f;
    [Export] protected float health = 5;
    [Export] protected float maxHealth = 100;


    protected Node3D target;
    protected List<ITargetable> targets;
    protected List<ISupporter> supporters;
    protected ulong tsLastAttackMS;

    public TEAM Team { get => team; set => SetTeam(value); }
    public double NormalizedHealth => Math.Clamp(health / maxHealth, 0.0, 1.0);
    public Node3D Body => this;

    public float Health { get => health; set => health = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public bool CanTakeDamage { get => canTakeDamage; set => canTakeDamage = value; }


    [ExportGroup("Team Meshes")]
    [Export] protected MeshInstance3D[] models;


    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            targets = new List<ITargetable>();
            supporters = new List<ISupporter>();
            health = maxHealth;
        }
    }
    #region BaseClass Public Virtual
       public virtual void Die()
    {
        Core.Rules.BuildingDied(this);
    }
    #endregion

    #region BaseClass Public
    public void AddSupporter(ISupporter supporter)
    {
        if (!supporters.Exists(p => p.GetInstanceId() == supporter.GetInstanceId()))
        {
            supporters.Add(supporter);
            supporter.TreeExiting += () => { RemoveSupporter(supporter); };
            UpdateUnitScale();
        }
    }
    public void RemoveSupporter(ISupporter supporter)
    {
        if (supporters.Exists(p => p.GetInstanceId() == supporter.GetInstanceId()))
        {
            supporters.RemoveAll(p => p.GetInstanceId() == supporter.GetInstanceId());
        }
        UpdateUnitScale();
    }
    #endregion

    #region BaseClass Private Virtual
 
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

    #endregion

    #region BaseClass Privates
    private protected int BuildDamage(int damage)
    {
        int extraDamage = 0;

        foreach (ISupporter supporter in supporters)
        {
            extraDamage += supporter.BaseDamageBonus(damage);
        }
        return damage + extraDamage;
    }
    private protected void SetTarget(ITargetable targetable)
    {
        if (targetable is UnitBaseClass) { target = targetable as UnitBaseClass; return; }
        if (targetable is BuildingBaseClass) { target = targetable as StaticBody3D; return; }
        if (targetable is PlayerAvatar) { target = targetable as RigidBody3D; return; }
    }
    private protected void SetTeam(TEAM value)
    {
        if (models is not null && models.Length > 0)
        {
            foreach (MeshInstance3D model in models)
            {
                model.SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
            }
        }
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }
    private protected void UpdateUnitScale()
    {
        float totalScale = 1.0f;
        foreach (ISupporter supporter in supporters)
        {
            totalScale += supporter.BaseScaleBonus();
        }
        GetNode<MeshInstance3D>("MeshInstance3D").Scale = Vector3.One * totalScale;
    }
    #endregion
}// EOF CLASS



