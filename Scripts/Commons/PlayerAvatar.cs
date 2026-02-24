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

    [Export] public PLAYERMODE mode = PLAYERMODE.NONE;
    [Export] float moveSpeed = 5.0f;
    [Export] float acceleration = 40.0f;
    [Export] Node3D placer;
    [Export] Node3D placerPoint;
    [Export] MeshInstance3D placeGhost;



    [ExportGroup("Combat Related")]
    [Export] Node3D weaponPivot;
    [Export] Node3D weaponMuzzle;
    [Export] int damage = 10;
    [Export] ulong attackCoolDownMS = 300;
    [Export] PackedScene projectilePrefab;
    [Export] float projectileSpeed = 20.0f;
    [Export] float projectileDuration = 2.0f;
    ulong TSLastAttackMS = 0;

    private int towerIDX = 0;


    private Vector3 inLeftStick;
    private Vector3 inRightStick;
    private Vector3 cursorWorldPosition;

    public TEAM Team { get => team; set => SetTeam(value); }

    public Node3D Body => this;

    private void SetTeam(TEAM value)
    {
        GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        CollisionLayer = value == TEAM.LEFT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }
    public override void _EnterTree()
    {
        Core.Rules.OnGameStart += WhenGameStarts;
    }

    private void WhenGameStarts(object sender, EventArgs e)
    {
        mode = PLAYERMODE.ATTACKING;
        placer.Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!GetWindow().HasFocus() || !IsMultiplayerAuthority()) { return; }
        // Building input vector Left stick
        inLeftStick = Vector3.Zero;
        inLeftStick += Vector3.Right * Input.GetAxis("LSLeft", "LSRight");
        inLeftStick += Vector3.Back * Input.GetAxis("LSUp", "LSDown");
        // Building input vector Right stick
        inRightStick = Vector3.Zero;
        inRightStick += Vector3.Right * Input.GetAxis("RSLeft", "RSRight");
        inRightStick += Vector3.Back * Input.GetAxis("RSUp", "RSDown");


        // Read toggle mode
        if (Input.IsActionJustPressed("TogglePlayerMode"))
        {
            mode = mode == PLAYERMODE.ATTACKING ? PLAYERMODE.BUILDING : PLAYERMODE.ATTACKING;
            if (mode == PLAYERMODE.ATTACKING) { weaponMuzzle.Show(); placer.Hide(); } else { weaponMuzzle.Hide(); }
        }

        // If in build mode accept tower index shift
        if (mode == PLAYERMODE.BUILDING)
        {
            if (Input.IsActionJustPressed("BuildSelectLeft"))
            {
                towerIDX--;
                if (towerIDX < 0) { towerIDX = Core.Rules.towers.MaxIndex; }
            }
            if (Input.IsActionJustPressed("BuildSelectRight"))
            {
                towerIDX++;
                if (towerIDX > Core.Rules.towers.MaxIndex) { towerIDX = 0; }
            }
        }



        // Mode dependent Controller
        if (inRightStick != Vector3.Zero)
        {
            // Rotate player
            weaponPivot.LookAt(GlobalPosition + inRightStick.Normalized() * 10.0f, Vector3.Up);
            if (mode == PLAYERMODE.BUILDING)
            {
                ProjectPlacerPosition(placer.GlobalPosition.DirectionTo(GlobalPosition + inRightStick.Normalized()));
            }
            else
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
            if (mode == PLAYERMODE.BUILDING)
            {
                ProjectPlacerPosition(placer.GlobalPosition.DirectionTo(cursorWorldPosition));
            }
            else
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
            if (mode == PLAYERMODE.BUILDING)
            {
                ProjectPlacerPosition(inRightStick.Normalized());
            }
            else
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
    }

    private void ProjectPlacerPosition(Vector3 direction)
    {
        Vector2I playerTileCoord = GridManager.WorldToCoord(GlobalPosition);
        Vector2I placerPointCoord = playerTileCoord;
        //Vector3 playerTileWorldPoint = GridManager.CoordToWorld(playerTileCoord);

        //MGizmosCSharp.GizmoUtils.DrawShape(placer.GlobalPosition + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.Pink);
        //MGizmosCSharp.GizmoUtils.DrawShape(GridManager.CoordToWorld(playerTileCoord), MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.BlueViolet);


        float angle = Vector3.Back.SignedAngleTo(direction, Vector3.Up) + Mathf.Pi;
        angle = Mathf.RadToDeg(angle);
        int step = Mathf.FloorToInt(angle / 45.0f);
        //GD.Print($"Angle[{angle}] step[{step}] playerTileCoord[{playerTileCoord}] coord to world pos difference[{(GlobalPosition - GridManager.CoordToWorld(playerTileCoord)).Length()}]");
        if(step < 1){ placerPointCoord += Vector2I.Up; }
        else if(step < 2){ placerPointCoord += Vector2I.Up + Vector2I.Left; }
        else if(step < 3){ placerPointCoord += Vector2I.Left; }
        else if(step < 4){ placerPointCoord += Vector2I.Down + Vector2I.Left; }
        else if(step < 5){ placerPointCoord += Vector2I.Down; }
        else if(step < 6){ placerPointCoord += Vector2I.Down + Vector2I.Right; }
        else if(step < 7){ placerPointCoord += Vector2I.Right; }
        else if(step < 8){ placerPointCoord += Vector2I.Up + Vector2I.Right; }
        else { placerPointCoord += Vector2I.Up; }



        placer.LookAt(placer.GlobalPosition + direction, Vector3.Up);
        //placerPoint.GlobalPosition = GridManager.SnapWorldToTileCenter(GlobalPosition + direction.Normalized() * 0.7f);
        placerPoint.GlobalPosition = GridManager.CoordToWorld(placerPointCoord);
        placerPoint.GlobalRotation = Vector3.Zero;
        RpcId(1, nameof(RPCRunPlacement), towerIDX);
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
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RPCRunPlacement(int towerIDX)
    {
        TowerResource tw = Core.Rules.towers.GetTowerByIndex(towerIDX);
        if (player.CanPay(tw.cost))
        {
            RpcId(player.PeerID, nameof(RPCSetPlacerState), true);
        }
        else
        {
            RpcId(player.PeerID, nameof(RPCSetPlacerState), false);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RPCSetPlacerState(bool value)
    {
        if (value) { ShowPlacement(); } else { placer.Hide(); }
    }


    private void ShowPlacement()
    {
        TowerResource tw = Core.Rules.towers.GetTowerByIndex(towerIDX);
        GridLocation gridLocation = GridManager.GetGridLocation(placeGhost.GlobalPosition);

        if (!gridLocation.CanFit(tw))
        {
            placer.Hide();
            return;
        }

        if(tw.towerType == TOWERTYPE.ATTACK)
        {
            if(gridLocation.Foundation is not null)
            {
                placeGhost.GlobalPosition = placerPoint.GlobalPosition + Vector3.Up * 0.669f;
            }
            else
            {
                placeGhost.GlobalPosition = placerPoint.GlobalPosition;
            }
            
        }
        placeGhost.Mesh = tw.mesh;
        placer.Show();
        if (Input.IsActionJustPressed("Place"))
        {
            Core.Rules.PlaceTower(
                Multiplayer.GetUniqueId(),
                team,
                towerIDX,
                placerPoint.GlobalPosition);
        }
    }

    public void TakeDamage(int damage)
    {
        player.TakeDamage(damage);
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





}// EOF CLASS
