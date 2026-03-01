using Godot;
using MLobby;
using OOTA;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Interfaces;
using OOTA.Resources;
using OOTA.Spawners;
using System;
using System.Collections.Generic;
[GlobalClass]
public partial class PlayerAvatar : RigidBody3D, ITargetable
{
    [Export] private TEAM team = TEAM.NONE;
    /// <summary>
    /// Host side only, reference back to player object that controls the avatar
    /// </summary>
    public OOTAPlayer player;

    [Export] float moveSpeed = 5.0f;
    [Export] float acceleration = 40.0f;
    [Export] protected int maxNBofSupporters = 4;


    public bool CanTakeDamage { get => player.CanTakeDamage; set => player.CanTakeDamage = value; }



    [ExportGroup("Jump")]
    [Export] float jumpForce = 8.0f;
    [Export] float jumpCoolDownMS = 1500;
    private ulong tsLastJumpMS = 0;



    [ExportGroup("Combat Related")]
    [Export] Node3D weaponPivot;
    [Export] Node3D weaponMuzzle;
    [Export] int damage = 10;
    [Export] ulong attackCoolDownMS = 300;
    [Export] PackedScene projectilePrefab;
    [Export] float projectileSpeed = 20.0f;
    [Export] float projectileDuration = 2.0f;
    ulong TSLastAttackMS = 0;



    private Vector3 inLeftStick;
    private Vector3 inRightStick;
    private Vector3 cursorWorldPosition;

    public TEAM Team { get => team; set => SetTeam(value); }

    public Node3D Body => this;

    public float Health { get => player.Health; set => player.SetHealth(value); }
    public float MaxHealth { get => player.MaxHealth; set => player.SetMaxHealth(value); }
    public List<ISupporter> Supporters => supporters;
    public bool CanBeSupported => supporters.Count < maxNBofSupporters;
    public float CurrentSpeed => LinearVelocity.Length();


    private void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }
    public override void _EnterTree()
    {
        Core.Rules.OnGameStart += WhenGameStarts;
        if (IsMultiplayerAuthority())
        {
            LocalLogic.OnPlayerModeChanged += WhenPlayerModeChanged;
        }
    }

    private void WhenPlayerModeChanged(object sender, PLAYERMODE mode)
    {
        if (mode == PLAYERMODE.ATTACKING)
        { weaponMuzzle.GetParent<Node3D>().Show(); }
        else
        { weaponMuzzle.GetParent<Node3D>().Hide(); }
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
        player.Mode = PLAYERMODE.ATTACKING;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!GetWindow().HasFocus() || !IsMultiplayerAuthority()) { return; }
        // Conserve fall speed
        float fallSpeed = LinearVelocity.Y;
        // Building input vector Left stick
        inLeftStick = Vector3.Zero;
        inLeftStick += Vector3.Right * Input.GetAxis("LSLeft", "LSRight");
        inLeftStick += Vector3.Back * Input.GetAxis("LSUp", "LSDown");
        // Building input vector Right stick
        inRightStick = Vector3.Zero;
        inRightStick += Vector3.Right * Input.GetAxis("RSLeft", "RSRight");
        inRightStick += Vector3.Back * Input.GetAxis("RSUp", "RSDown");




        // Mode dependent Controller
        if (inRightStick != Vector3.Zero)
        {
            // Rotate player
            weaponPivot.LookAt(GlobalPosition + inRightStick.Normalized() * 10.0f, Vector3.Up);
            if (player.Mode != PLAYERMODE.BUILDING)
            {
                if (!Multiplayer.IsServer()) { RpcId(1, nameof(RPCRunAttack)); } else { RPCRunAttack(); }
            }
        }
        else if (inRightStick == Vector3.Zero && Core.PlotMouseWorldPosition(out cursorWorldPosition))
        // Mode dependent Mouse
        {
            // Rotate player
            cursorWorldPosition.Y = GlobalPosition.Y;
            weaponPivot.LookAt(cursorWorldPosition, Vector3.Up);
            if (player.Mode != PLAYERMODE.BUILDING)
            {
                if (Input.IsActionPressed("Attack"))
                {
                    RpcId(1, nameof(RPCRunAttack));
                }
            }
        }
        else if (inRightStick != Vector3.Zero)
        {
            // Rotate player
            weaponPivot.LookAt(GlobalPosition + inRightStick.Normalized() * 10.0f, Vector3.Up);
            if (player.Mode != PLAYERMODE.BUILDING)
            {
                if (!Multiplayer.IsServer()) { RpcId(1, nameof(RPCRunAttack)); } else { RPCRunAttack(); }
            }
        }


        if (inLeftStick != Vector3.Zero)
        {
            if (inLeftStick.Dot(LinearVelocity) < 0.2f)
            {
                //GD.Print($"Breaking!  DOT[{inLeftStick.Dot(LinearVelocity)}] inLeftStick[{inLeftStick.Length()}] LinearVelocity[{LinearVelocity.Length()}]");
                LinearVelocity *= 0.5f;
            }
            ApplyForce(inLeftStick * Mass * acceleration * SpeedModifier());
        }
        else
        {
            LinearVelocity *= 0.85f;
        }

        // Reapply the conserved fall speed
        LinearVelocity = new Vector3(LinearVelocity.X, fallSpeed, LinearVelocity.Z);
        // Jump
        if (Input.IsActionJustPressed("Jump") && Time.GetTicksMsec() > tsLastJumpMS + jumpCoolDownMS)
        {
            tsLastJumpMS = Time.GetTicksMsec();
            ApplyImpulse(Vector3.Up * jumpForce * Mass);
        }
    }








    //[Obsolete("Verify this overrides the interface default method")]
    public virtual void TakeDamage(float damage)
    {
        //GD.Print($"PlayerAvatar::TakeDamage({damage}) called and CanTakeDamage is {CanTakeDamage}");
        player.TakeDamage(damage);
    }


    public virtual void Die()
    {
        player.Die();
    }

    private float SpeedModifier()
    {
        if (LinearVelocity != Vector3.Zero)
        {
            return 1.0f - LinearVelocity.Length() / moveSpeed;
        }
        return 1.0f;
    }


    public List<ISupporter> supporters;

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

    internal void AddHealth(int health)
    {
        player.AddHealth(health);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RPCRunAttack()
    {
        if (Time.GetTicksMsec() > TSLastAttackMS + attackCoolDownMS)
        {
            TSLastAttackMS = Time.GetTicksMsec();
            Projectile proj = ProjectileSpawner.SpawnThisProjectile(new ProjectileSpawner.SpawnProjectileArgument("res://Scenes/Projectiles/PlayerBaseProjectile.tscn",
                team, damage, projectileSpeed, projectileDuration, GetPath(), weaponMuzzle.GlobalRotation, weaponMuzzle.GlobalPosition
            ));
        }
    }




}// EOF CLASS
