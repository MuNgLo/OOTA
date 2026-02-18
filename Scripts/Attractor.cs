using Godot;
using System;
using System.Collections.Generic;

public partial class Attractor : Area3D
{
    [Export] float attractionForce = 20.0f;
    [Export] float pickUpRange = 1.0f;
    [Export] PlayerAvatar pl;
    List<RigidBody3D> bodies;

    public override void _Ready()
    {
        if (!Multiplayer.IsServer()) { ProcessMode = ProcessModeEnum.Disabled; return; }
        bodies = new List<RigidBody3D>();
        BodyEntered += WhenBodyEntered;
        BodyExited += WhenBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (RigidBody3D body in bodies)
        {
            body.ApplyForce(body.GlobalPosition.DirectionTo(pl.GlobalPosition) * attractionForce);
            if (body.GlobalPosition.DistanceTo(pl.GlobalPosition) < pickUpRange)
            {
                if (body is Pickup pickup)
                {
                    Core.Rules.PlayerAddGold(GetParent().GetMultiplayerAuthority(), pickup.gold);
                    Core.Rules.PlayerAddHealth(GetParent().GetMultiplayerAuthority(), pickup.health);
                }
                body.QueueFree();
            }
        }
    }

    private void WhenBodyEntered(Node3D body)
    {
        if (!bodies.Contains(body as RigidBody3D))
        {
            bodies.Add(body as RigidBody3D);
        }
    }
    private void WhenBodyExited(Node3D body)
    {
        if (bodies.Contains(body as RigidBody3D))
        {
            bodies.Remove(body as RigidBody3D);
        }
    }
}// EOF CLASS
