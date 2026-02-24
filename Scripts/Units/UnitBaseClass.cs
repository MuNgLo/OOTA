using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOTA.Units;

[GlobalClass]
public partial class UnitBaseClass : RigidBody3D, ITargetable, IMind
{
    /// <summary>
    /// None: Not doing anything<br/>
    /// Traveling: Moving along a path<br/>
    /// Hunting: Moving relative to target
    /// </summary>
    public enum MINDSTATE { NONE, TRAVELING, HUNTING }
    public enum PATHSTATE { IDLE, PENDING, EXECUTING, FINISHED }
    [Export] protected TEAM team = TEAM.NONE;

    [ExportGroup("States")]
    [Export] protected MINDSTATE mindState = MINDSTATE.NONE;
    [Export] protected PATHSTATE pathState = PATHSTATE.IDLE;

    [ExportGroup("Movement")]
    [Export] float moveSpeed = 8.0f;
    [Export] protected float acceleration = 80.0f;
    [Export] protected float aggroRange = 6.0f;



    [ExportGroup("Attack")]
    [Export] protected int damage = 3;
    [Export] protected bool canAttackBuildings = false;
    [Export] protected ulong attackCoolDownMS = 300;
    [Export] protected float attackRange = 1.0f;
    [Export] float targetMinDistance = 3.0f;


    [ExportGroup("Health")]
    [Export] float health = 100;
    [Export] float maxHealth = 100;


    protected Node3D target;
    protected int pathIndex = 0;
    protected Vector3 inVec;
    protected List<ITargetable> targets;
    protected List<ISupporter> supporters;
    protected List<Vector3> path;
    protected ulong lastAttack;

    public double NormalizedHealth => Math.Clamp(health / maxHealth, 0.0, 1.0);
    public TEAM Team { get => team; set => SetTeam(value); }
    public Node3D Body => this;
    private protected Vector3 nextPathPoint => path[pathIndex] + Vector3.Down * 0.5f;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            path = new List<Vector3>();
            targets = new List<ITargetable>();
            supporters = new List<ISupporter>();
            GridNavigation.OnNavMeshRebuilt += WhenNavMeshRebuilt;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Skip on non Host
        if (!Multiplayer.IsServer()) { return; }

        switch (mindState)
        {
            case MINDSTATE.HUNTING:
                ProcessHunting((float)delta);
                break;
            case MINDSTATE.TRAVELING:
                ProcessTraveling((float)delta);
                break;
            default:
                ProcessNone((float)delta);
                break;
        }
    }

    #region BaseClass Public Virtual
    public virtual void AddHealth(int amount)
    {
        if (amount < 1) { return; }
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }
    public virtual void AttackTarget()
    {
        if (target is UnitBaseClass unit) { unit.TakeDamage(BuildDamage(damage)); return; }
        if (target is BuildingBaseClass building) { building.TakeDamage(BuildDamage(damage)); return; }
        if (target is PlayerAvatar avatar) { avatar.TakeDamage(BuildDamage(damage)); return; }
    }
    public virtual void BodyEnteredAggroRange(Node3D body)
    {
        if (body is ITargetable t && t.Team != team)
        {
            if(body is BuildingBaseClass && !canAttackBuildings) { return; }
            if (!targets.Contains(t))
            {
                targets.Add(t);
                ResetMind();
            }
        }
    }
    public virtual void BodyExitedAggroRange(Node3D body)
    {
        if (body is ITargetable t)
        {
            if (targets.Contains(t))
            {
                targets.Remove(t);
            }
            if (target is not null && target == body)
            {
                ResetMind();
            }
        }
    }
    public virtual void Die()
    {
        GridNavigation.OnNavMeshRebuilt -= WhenNavMeshRebuilt;
        Core.Rules.UnitDied(this);
    }
    public virtual void ProcessHunting(float delta)
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

        // Attack target if close enough
        if (TargetInRange())
        {
            if (LinearVelocity != Vector3.Zero) { LinearVelocity *= 0.5f; }
            if (Time.GetTicksMsec() > lastAttack + attackCoolDownMS)
            {
                lastAttack = Time.GetTicksMsec();
                AttackTarget();
            }
        }
    }
    public virtual void ProcessNone(float delta)
    {
        PickTarget();
        if (target is Base)
        {
            mindState = MINDSTATE.TRAVELING;
            return;
        }
        mindState = MINDSTATE.HUNTING;
    }
    public virtual void ProcessTraveling(float delta)
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
    public virtual void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }
    public virtual void TakeDamage(int amount)
    {
        if (amount < 1) { return; }
        health -= amount;
        if (health <= 0.0f) { Die(); }
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

    private protected virtual bool TargetIsToClose()
    {
        return GlobalPosition.DistanceTo(target.GlobalPosition) < targetMinDistance;
    }
    /// <summary>
    /// Default behavior is to pick closest target. If that fails, target enemy base.
    /// </summary>
    private protected virtual void PickTarget()
    {
        if (targets.Count > 0)
        {
            List<ITargetable> candidates = targets.FindAll(p => p.Team != team).OrderBy(p => p.GlobalPosition.DistanceTo(GlobalPosition)).ToList();
            if (candidates.Count > 0)
            {
                if (target != candidates[0])
                {
                    SetTarget(candidates[0]);
                }
                return;
            }
        }
        TargetEnemyBase();
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
    private protected async void GetPathToTarget()
    {
        //GD.Print("UnitBaseClass::GetPathToTarget()");
        // Block if we have a query pending
        if (pathState == PATHSTATE.PENDING) { return; }
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        if (target is null) { ResetMind(); return; }
        Vector3[] arr = NavigationServer3D.MapGetPath(GridNavigation.WorldNavMapRid, GlobalPosition, target.GlobalPosition, true);
        path.Clear();
        path.AddRange(arr);
        pathIndex = 0;
        //MGizmosCSharp.GizmoUtils.DrawShape(GlobalPosition + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 5.0f, 0.5f, Colors.Red);
        //MGizmosCSharp.GizmoUtils.DrawShape(path.Last() + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 5.0f, 0.5f, Colors.Red);
        if (path.Count > 0)
        {
            pathState = PATHSTATE.EXECUTING;
            return;
        }
        target = null;
        pathState = PATHSTATE.IDLE;
    }
    private protected void ResetMind()
    {
        mindState = MINDSTATE.NONE;
        pathState = PATHSTATE.IDLE;
        path.Clear();
        pathIndex = 0;
        target = null;
    }
    private protected void SetTarget(ITargetable targetable)
    {
        if (targetable is UnitBaseClass) { target = targetable as UnitBaseClass; path.Clear(); pathState = PATHSTATE.IDLE; return; }
        if (targetable is BuildingBaseClass) { target = targetable as StaticBody3D; path.Clear(); pathState = PATHSTATE.IDLE; return; }
        if (targetable is PlayerAvatar) { target = targetable as RigidBody3D; path.Clear(); pathState = PATHSTATE.IDLE; return; }
    }
    private protected float SpeedModifier()
    {
        if (LinearVelocity != Vector3.Zero)
        {
            return 1.0f - LinearVelocity.Length() / moveSpeed;
        }
        return 1.0f;
    }
    private protected void TargetEnemyBase()
    {
        target = team == TEAM.LEFT ? GridManager.RightTeamGoal : GridManager.LeftTeamGoal;
        pathState = PATHSTATE.IDLE;
    }
    private protected void TargetFriendlyBase()
    {
        target = team == TEAM.LEFT ? GridManager.LeftTeamGoal : GridManager.RightTeamGoal;
        pathState = PATHSTATE.IDLE;
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
    private void UpdateUnitScale()
    {
        if (!IsInstanceValid(this) || !Multiplayer.HasMultiplayerPeer()) { return; }
        float totalScale = 1.0f;
        foreach (ISupporter supporter in supporters)
        {
            totalScale += supporter.BaseScaleBonus();
        }
        Rpc(nameof(RPCSetUnitScale), totalScale);
    }

    private protected void WhenNavMeshRebuilt(object sender, EventArgs e)
    {
        ResetMind();
    }
    #endregion





    #region  RPCs
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCSetUnitScale(float totalScale)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").Scale = Vector3.One * totalScale;
        GetNode<CollisionShape3D>("CollisionShape3D").Scale = Vector3.One * totalScale;
    }
    #endregion









}// EOF CLASS