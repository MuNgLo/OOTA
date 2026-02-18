using Godot;
using MLogging;
using System;

public partial class Projectile : ShapeCast3D
{
    [Export] private TEAM team = TEAM.NONE;
    public TEAM Team { get => team; set => SetTeam(value); }

    public double moveSpeed;
    public double timeToLive;
    public int damage;

    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.IsServer())
        {
            TargetPosition = Vector3.Forward * (float)moveSpeed * (float)delta;
            ForceShapecastUpdate();
            if (IsColliding())
            {
                // Hit something
                GodotObject obj = GetCollider(0);
                if (obj is ITargetable targetable)
                {
                    //MLog.LogInfo($"Projectile::_PhysicsProcess() Hit a UnitBaseClass for damage[{damage}]");
                    targetable.TakeDamage(damage);
                    if(targetable is RigidBody3D rb)
                    {
                        //rb.ApplyCentralImpulse((-GlobalBasis.Z + Vector3.Up * 0.3f).Normalized() * 15.0f);
                    }
                }
                Die();
            }
            else
            {
                Position += -GlobalBasis.Z * (float)moveSpeed * (float)delta;
            }
            timeToLive -= delta;
            if (timeToLive <= 0.0f) { Die(); }

        }
        else
        {
            Position += -GlobalBasis.Z * (float)moveSpeed * (float)delta;
        }
    }

    private void Die()
    {
        QueueFree();
    }

    private void SetTeam(TEAM value)
    {
        //GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        team = value;
        CollisionMask = value == TEAM.RIGHT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
    }
}// EOF CLASS
