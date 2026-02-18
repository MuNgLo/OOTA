using Godot;
using System;

public partial class Pickup : RigidBody3D
{
    [Export] private TEAM team = TEAM.NONE;

    [Export] public int health = 0;
    [Export] public int gold = 0;

    public TEAM Team { get => team; set => SetTeam(value); }

    private void SetTeam(TEAM value)
    {
        //GetNode<MeshInstance3D>("MeshInstance3D").SetSurfaceOverrideMaterial(0, Core.TeamMaterial(value));
        //team = value;
    }

}// EOF CLASS
